using ClinicHub.Infrastructure.Services.Interfaces;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json.Serialization;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services
{
    public class GoogleMapService : IMapService
    {
        private const string NearbyFieldMask = "places.id,places.displayName,places.formattedAddress,places.location,places.rating,places.nationalPhoneNumber,places.websiteUri";
        private const string TextSearchFieldMask = "places.id,places.displayName,places.formattedAddress,places.location,places.rating,places.nationalPhoneNumber,places.websiteUri";
        private const string RouteFieldMask = "routes.distanceMeters,routes.duration,routes.polyline.encodedPolyline";
        private const int MaxResultsPerPage = 20;
        private const int MaxTotalResults = 60;

        private static readonly string[] HealthcareTypes =
        [
            "hospital",
            "doctor",
            "dentist",
            "medical_center",
            "physiotherapist",
            "diagnostic_center",
            "laboratory",
            "radiology_center"
        ];

        private static readonly string[][] NearbyTypeGroups =
        [
            ["hospital", "medical_center"],
            ["doctor", "dentist"],
            ["physiotherapist", "diagnostic_center", "laboratory", "radiology_center"]
        ];

        private readonly HttpClient _httpClient;
        private readonly GoogleMapsSettings _options;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GoogleMapService> _logger;

        public GoogleMapService(HttpClient httpClient, IOptions<GoogleMapsSettings> options, IMemoryCache cache, ILogger<GoogleMapService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<ClinicExternalDto>> GetNearbyFromMapAsync(double lat, double lng, string category, CancellationToken cancellationToken, double radius = 5000, string? languageCode = null)
        {
            var lang = languageCode ?? "en";
            var cacheKey = $"Google_Nearby_{lat}_{lng}_{category}_{radius}_{lang}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                _logger.LogDebug("Cache hit for NearbySearch: {CacheKey} ({Count} results)", cacheKey, cachedResults?.Count ?? 0);
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            var includedTypes = NormalizePlaceTypes(category).ToList();
            string[][] typeGroups = SplitIntoTypeGroups(includedTypes);

            _logger.LogInformation("Google NearbySearch: lat={Lat}, lng={Lng}, {Groups} group(s), radius={Radius}m, lang={Lang}",
                lat, lng, typeGroups.Length, radius, lang);

            var allResults = new List<ClinicExternalDto>();
            var seenPlaceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var groupTasks = typeGroups.Select(async groupTypes =>
            {
                var groupResults = new List<ClinicExternalDto>();
                string? nextPageToken = null;
                var pageCount = 0;

                do
                {
                    var requestBody = new GoogleNearbySearchRequest
                    {
                        IncludedTypes = [.. groupTypes],
                        LanguageCode = lang,
                        MaxResultCount = MaxResultsPerPage,
                        LocationRestriction = new GoogleLocationRestriction
                        {
                            Circle = new GoogleCircle
                            {
                                Center = new GoogleLatLng
                                {
                                    Latitude = lat,
                                    Longitude = lng
                                },
                                Radius = radius
                            }
                        }
                    };

                    if (!string.IsNullOrEmpty(nextPageToken))
                    {
                        requestBody.PageToken = nextPageToken;
                    }

                    try
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Post, _options.NearByFromMapBaseUrl);
                        request.Headers.Add("X-Goog-Api-Key", _options.ApiKey);
                        request.Headers.Add("X-Goog-FieldMask", NearbyFieldMask);
                        request.Content = JsonContent.Create(requestBody);

                        var response = await _httpClient.SendAsync(request, cancellationToken);
                        var body = await response.Content.ReadAsStringAsync(cancellationToken);

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError("Google NearbySearch [{Types}] failed ({Status}): {Body}",
                                string.Join(",", groupTypes), response.StatusCode, body);
                            break;
                        }

                        var payload = await response.Content.ReadFromJsonAsync<GoogleNearbySearchResponse>(cancellationToken: cancellationToken);

                        if (payload?.Error != null)
                        {
                            _logger.LogError("Google NearbySearch [{Types}] API error {Code}: {Message}",
                                string.Join(",", groupTypes), payload.Error.Code, payload.Error.Message);
                            break;
                        }

                        if (payload?.Places is not null && payload.Places.Count > 0)
                        {
                            _logger.LogDebug("Google NearbySearch [{Types}] page {Page}: got {Count} results",
                                string.Join(",", groupTypes), pageCount + 1, payload.Places.Count);

                            foreach (var place in payload.Places)
                            {
                                var dto = MapPlaceToDto(place, lang);
                                if (!string.IsNullOrEmpty(dto.PlaceId) && !seenPlaceIds.Add(dto.PlaceId))
                                    continue;
                                groupResults.Add(dto);
                            }

                            nextPageToken = payload.NextPageToken;
                            pageCount++;
                        }
                        else
                        {
                            nextPageToken = null;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Google NearbySearch [{Types}] error: {Message}",
                            string.Join(",", groupTypes), ex.Message);
                        break;
                    }

                    if (!string.IsNullOrEmpty(nextPageToken))
                    {
                        await Task.Delay(500, cancellationToken);
                    }
                }
                while (!string.IsNullOrEmpty(nextPageToken) && groupResults.Count < MaxTotalResults && pageCount < 3);

                _logger.LogDebug("Google NearbySearch [{Types}] complete: {Count} results across {Pages} page(s)",
                    string.Join(",", groupTypes), groupResults.Count, pageCount);

                return groupResults;
            });

            var nestedResults = await Task.WhenAll(groupTasks);
            foreach (var groupResult in nestedResults)
            {
                allResults.AddRange(groupResult);
            }

            _logger.LogInformation("Google NearbySearch complete: total {Count} results from {Groups} type group(s)",
                allResults.Count, typeGroups.Length);

            if (allResults.Count > 0)
            {
                _cache.Set(cacheKey, allResults, TimeSpan.FromMinutes(30));
            }

            return allResults;
        }

        public async Task<List<ClinicExternalDto>> TextSearchAsync(string query, double? lat, double? lng, double radius, CancellationToken cancellationToken, string? languageCode = null)
        {
            var lang = languageCode ?? "en";
            var cacheKey = $"Google_TextSearch_{query}_{lat}_{lng}_{radius}_{lang}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                _logger.LogDebug("Cache hit for TextSearch: {CacheKey} ({Count} results)", cacheKey, cachedResults?.Count ?? 0);
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            if (string.IsNullOrWhiteSpace(_options.TextSearchBaseUrl))
            {
                _logger.LogWarning("TextSearchBaseUrl not configured, falling back to NearbySearch");
                if (lat.HasValue && lng.HasValue)
                {
                    return await GetNearbyFromMapAsync(lat.Value, lng.Value, string.Join(",", HealthcareTypes), cancellationToken, radius, lang);
                }
                return new List<ClinicExternalDto>();
            }

            _logger.LogInformation("Google TextSearch: query=\"{Query}\", lat={Lat}, lng={Lng}, radius={Radius}m, lang={Lang}",
                query, lat, lng, radius, lang);

            var allResults = new List<ClinicExternalDto>();
            string? nextPageToken = null;
            var pageCount = 0;

            do
            {
                var requestBody = new GoogleTextSearchRequest
                {
                    TextQuery = query,
                    LanguageCode = lang,
                    MaxResultCount = MaxResultsPerPage
                };

                if (lat.HasValue && lng.HasValue)
                {
                    requestBody.LocationBias = new GoogleLocationBias
                    {
                        Circle = new GoogleCircle
                        {
                            Center = new GoogleLatLng
                            {
                                Latitude = lat.Value,
                                Longitude = lng.Value
                            },
                            Radius = radius > 0 ? radius : 50000
                        }
                    };
                }

                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    requestBody.PageToken = nextPageToken;
                }

                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, _options.TextSearchBaseUrl);
                    request.Headers.Add("X-Goog-Api-Key", _options.ApiKey);
                    request.Headers.Add("X-Goog-FieldMask", TextSearchFieldMask);
                    request.Content = JsonContent.Create(requestBody);

                    var response = await _httpClient.SendAsync(request, cancellationToken);
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Google TextSearch failed ({Status}): {Body}", response.StatusCode, body);
                        break;
                    }

                    var payload = await response.Content.ReadFromJsonAsync<GoogleTextSearchResponse>(cancellationToken: cancellationToken);

                    if (payload?.Error != null)
                    {
                        _logger.LogError("Google TextSearch API error {Code}: {Message}",
                            payload.Error.Code, payload.Error.Message);
                        break;
                    }

                    if (payload?.Places is not null && payload.Places.Count > 0)
                    {
                        _logger.LogDebug("Google TextSearch page {Page}: got {Count} results", pageCount + 1, payload.Places.Count);

                        foreach (var place in payload.Places)
                        {
                            allResults.Add(MapPlaceToDto(place, lang));
                        }

                        nextPageToken = payload.NextPageToken;
                        pageCount++;
                    }
                    else
                    {
                        nextPageToken = null;
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Google TextSearch cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Google TextSearch error: {Message}", ex.Message);
                    break;
                }

                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    await Task.Delay(500, cancellationToken);
                }
            }
            while (!string.IsNullOrEmpty(nextPageToken) && allResults.Count < MaxTotalResults && pageCount < 3);

            _logger.LogInformation("Google TextSearch complete: total {Count} results across {Pages} page(s)", allResults.Count, pageCount);

            if (allResults.Count > 0)
            {
                _cache.Set(cacheKey, allResults, TimeSpan.FromMinutes(30));
            }

            return allResults;
        }

        public async Task<List<ClinicExternalDto>> GeocodeAsync(string address, CancellationToken cancellationToken, int limit = 10)
        {
            var cacheKey = $"Google_Geocode_{address}_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                _logger.LogDebug("Cache hit for Geocode: {CacheKey} ({Count} results)", cacheKey, cachedResults?.Count ?? 0);
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            var url = $"{_options.GeoCodeBaseUrl}/json?address={Uri.EscapeDataString(address)}&components=country:EG&key={_options.ApiKey}";

            _logger.LogInformation("Google Geocode: address=\"{Address}\"", address);

            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(url, cancellationToken);

                if (response?.Results != null && response.Results.Count > 0)
                {
                    _logger.LogDebug("Google Geocode got {Count} results", response.Results.Count);

                    var results = response.Results.Take(limit).Select(res => new ClinicExternalDto
                    {
                        Name = res.FormattedAddress?.Split(',')[0] ?? "Unknown",
                        Lat = res.Geometry?.Location?.Lat ?? 0,
                        Lng = res.Geometry?.Location?.Lng ?? 0,
                        Address = res.FormattedAddress
                    }).ToList();

                    _cache.Set(cacheKey, results, TimeSpan.FromHours(24));
                    return results;
                }

                _logger.LogWarning("Google Geocode returned no results for address: {Address}", address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Geocode error: {Message}", ex.Message);
            }

            return new List<ClinicExternalDto>();
        }

        public async Task<string?> ReverseGeocodeAsync(double lat, double lng, CancellationToken cancellationToken)
        {
            var url = $"{_options.GeoCodeBaseUrl}/json?latlng={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}&key={_options.ApiKey}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(url, cancellationToken);
                var result = response?.Results?.FirstOrDefault()?.FormattedAddress;
                _logger.LogDebug("ReverseGeocode ({Lat},{Lng}) -> {Result}", lat, lng, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google ReverseGeocode error: {Message}", ex.Message);
            }
            return null;
        }

        public async Task<RouteDto?> GetRouteAsync(double startLat, double startLng, double endLat, double endLng, CancellationToken cancellationToken)
        {
            var requestBody = new GoogleComputeRoutesRequest
            {
                Origin = new GoogleRouteWaypoint
                {
                    Location = new GoogleLocationWrapper
                    {
                        LatLng = new GoogleLatLng
                        {
                            Latitude = startLat,
                            Longitude = startLng
                        }
                    }
                },
                Destination = new GoogleRouteWaypoint
                {
                    Location = new GoogleLocationWrapper
                    {
                        LatLng = new GoogleLatLng
                        {
                            Latitude = endLat,
                            Longitude = endLng
                        }
                    }
                },
                TravelMode = "DRIVE",
                RoutingPreference = "TRAFFIC_UNAWARE",
                ComputeAlternativeRoutes = false,
                Units = "METRIC"
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _options.RoutesBaseUrl);
                request.Headers.Add("X-Goog-Api-Key", _options.ApiKey);
                request.Headers.Add("X-Goog-FieldMask", RouteFieldMask);
                request.Content = JsonContent.Create(requestBody);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var payload = await response.Content.ReadFromJsonAsync<GoogleRoutesResponse>(cancellationToken: cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Google Routes failed ({Status})", response.StatusCode);
                    return null;
                }

                var route = payload?.Routes?.FirstOrDefault();
                if (route is null)
                {
                    _logger.LogWarning("Google Routes returned no routes");
                    return null;
                }

                _logger.LogDebug("Google Route: {Distance}m, {Duration}", route.DistanceMeters, route.Duration);

                return new RouteDto
                {
                    Distance = route.DistanceMeters,
                    Duration = ParseDurationToMinutes(route.Duration),
                    Geometry = DecodePolyline(route.Polyline?.EncodedPolyline)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Routes error: {Message}", ex.Message);
                return null;
            }
        }

        private static ClinicExternalDto MapPlaceToDto(GooglePlaceResult place, string languageCode)
        {
            var nameAr = languageCode == "ar"
                ? place.DisplayName?.Text
                : null;

            var nameEn = languageCode != "ar"
                ? place.DisplayName?.Text
                : null;

            return new ClinicExternalDto
            {
                PlaceId = place.Id ?? string.Empty,
                Name = nameEn ?? place.DisplayName?.Text ?? "Unknown Clinic",
                NameAr = nameAr,
                Lat = place.Location?.Latitude ?? 0,
                Lng = place.Location?.Longitude ?? 0,
                Address = place.FormattedAddress,
                AddressAr = languageCode != "ar" ? null : place.FormattedAddress,
                Phone = place.NationalPhoneNumber,
                Website = place.WebsiteUri,
                Rating = place.Rating
            };
        }

        private static string[][] SplitIntoTypeGroups(List<string> types)
        {
            if (types.Count == 0)
                return NearbyTypeGroups;

            if (types.Count <= 3)
                return [types.ToArray()];

            var hospitalLike = types.Where(t => t is "hospital" or "medical_center").ToArray();
            var doctorLike = types.Where(t => t is "doctor" or "dentist").ToArray();
            var specialized = types.Where(t =>
                t is not "hospital" and not "medical_center" and not "doctor" and not "dentist").ToArray();

            var groups = new List<string[]>();
            if (hospitalLike.Length > 0) groups.Add(hospitalLike);
            if (doctorLike.Length > 0) groups.Add(doctorLike);
            if (specialized.Length > 0) groups.Add(specialized);

            return groups.Count > 0 ? groups.ToArray() : [types.ToArray()];
        }

        private static IEnumerable<string> NormalizePlaceTypes(string category)
        {
            foreach (var type in category.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                yield return type.Trim() switch
                {
                    "doctors" => "doctor",
                    "medical_centre" => "medical_center",
                    "health_post" => "hospital",
                    "clinic" => "doctor",
                    "health" => "hospital",
                    _ => type.Trim()
                };
            }
        }

        private static double ParseDurationToMinutes(string? duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
            {
                return 0;
            }

            var secondsText = duration.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                ? duration[..^1]
                : duration;

            return double.TryParse(secondsText, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
                ? seconds / 60d
                : 0;
        }

        private static List<List<double>>? DecodePolyline(string? encodedPolyline)
        {
            if (string.IsNullOrWhiteSpace(encodedPolyline))
            {
                return null;
            }

            var polyline = new List<List<double>>();
            var index = 0;
            var latitude = 0;
            var longitude = 0;

            while (index < encodedPolyline.Length)
            {
                latitude += DecodeNextPolylineValue(encodedPolyline, ref index);
                longitude += DecodeNextPolylineValue(encodedPolyline, ref index);
                polyline.Add(new List<double> { longitude / 1E5, latitude / 1E5 });
            }

            return polyline;
        }

        private static int DecodeNextPolylineValue(string encodedPolyline, ref int index)
        {
            var result = 0;
            var shift = 0;
            int b;

            do
            {
                b = encodedPolyline[index++] - 63;
                result |= (b & 0x1f) << shift;
                shift += 5;
            }
            while (b >= 0x20);

            return (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
        }

        private class GoogleNearbySearchRequest
        {
            [JsonPropertyName("includedTypes")]
            public List<string> IncludedTypes { get; set; } = [];

            [JsonPropertyName("languageCode")]
            public string? LanguageCode { get; set; }

            [JsonPropertyName("maxResultCount")]
            public int MaxResultCount { get; set; }

            [JsonPropertyName("locationRestriction")]
            public GoogleLocationRestriction? LocationRestriction { get; set; }

            [JsonPropertyName("pageToken")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? PageToken { get; set; }
        }

        private class GoogleLocationRestriction
        {
            [JsonPropertyName("circle")]
            public GoogleCircle? Circle { get; set; }
        }

        private class GoogleCircle
        {
            [JsonPropertyName("center")]
            public GoogleLatLng? Center { get; set; }

            [JsonPropertyName("radius")]
            public double Radius { get; set; }
        }

        private class GoogleLatLng
        {
            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }
        }

        private class GoogleNearbySearchResponse
        {
            [JsonPropertyName("places")]
            public List<GooglePlaceResult>? Places { get; set; }

            [JsonPropertyName("nextPageToken")]
            public string? NextPageToken { get; set; }

            [JsonPropertyName("error")]
            public GoogleApiError? Error { get; set; }
        }

        private class GoogleTextSearchRequest
        {
            [JsonPropertyName("textQuery")]
            public string TextQuery { get; set; } = string.Empty;

            [JsonPropertyName("languageCode")]
            public string? LanguageCode { get; set; }

            [JsonPropertyName("maxResultCount")]
            public int MaxResultCount { get; set; }

            [JsonPropertyName("locationBias")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public GoogleLocationBias? LocationBias { get; set; }

            [JsonPropertyName("includedType")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? IncludedType { get; set; }

            [JsonPropertyName("pageToken")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? PageToken { get; set; }
        }

        private class GoogleLocationBias
        {
            [JsonPropertyName("circle")]
            public GoogleCircle? Circle { get; set; }
        }

        private class GoogleTextSearchResponse
        {
            [JsonPropertyName("places")]
            public List<GooglePlaceResult>? Places { get; set; }

            [JsonPropertyName("nextPageToken")]
            public string? NextPageToken { get; set; }

            [JsonPropertyName("error")]
            public GoogleApiError? Error { get; set; }
        }

        private class GooglePlaceResult
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("displayName")]
            public GoogleDisplayName? DisplayName { get; set; }

            [JsonPropertyName("formattedAddress")]
            public string? FormattedAddress { get; set; }

            [JsonPropertyName("location")]
            public GoogleGoogleLocation? Location { get; set; }

            [JsonPropertyName("rating")]
            public double? Rating { get; set; }

            [JsonPropertyName("nationalPhoneNumber")]
            public string? NationalPhoneNumber { get; set; }

            [JsonPropertyName("websiteUri")]
            public string? WebsiteUri { get; set; }
        }

        private class GoogleDisplayName
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

        private class GoogleGoogleLocation
        {
            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }
        }

        private class GoogleApiError
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }

        private class GoogleGeocodeResponse
        {
            public List<GoogleGeocodeResult>? Results { get; set; }
            public string? Status { get; set; }
            [JsonPropertyName("error_message")]
            public string? ErrorMessage { get; set; }
        }

        private class GoogleGeocodeResult
        {
            [JsonPropertyName("formatted_address")]
            public string? FormattedAddress { get; set; }
            public GoogleGeometry? Geometry { get; set; }
        }

        private class GoogleGeometry
        {
            public GoogleLocation? Location { get; set; }
        }

        private class GoogleLocation
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        private class GoogleComputeRoutesRequest
        {
            [JsonPropertyName("origin")]
            public GoogleRouteWaypoint? Origin { get; set; }

            [JsonPropertyName("destination")]
            public GoogleRouteWaypoint? Destination { get; set; }

            [JsonPropertyName("travelMode")]
            public string? TravelMode { get; set; }

            [JsonPropertyName("routingPreference")]
            public string? RoutingPreference { get; set; }

            [JsonPropertyName("computeAlternativeRoutes")]
            public bool ComputeAlternativeRoutes { get; set; }

            [JsonPropertyName("units")]
            public string? Units { get; set; }
        }

        private class GoogleRouteWaypoint
        {
            [JsonPropertyName("location")]
            public GoogleLocationWrapper? Location { get; set; }
        }

        private class GoogleLocationWrapper
        {
            [JsonPropertyName("latLng")]
            public GoogleLatLng? LatLng { get; set; }
        }

        private class GoogleRoutesResponse
        {
            [JsonPropertyName("routes")]
            public List<GoogleRouteResult>? Routes { get; set; }

            [JsonPropertyName("error")]
            public GoogleApiError? Error { get; set; }
        }

        private class GoogleRouteResult
        {
            [JsonPropertyName("distanceMeters")]
            public double DistanceMeters { get; set; }

            [JsonPropertyName("duration")]
            public string? Duration { get; set; }

            [JsonPropertyName("polyline")]
            public GooglePolyline? Polyline { get; set; }
        }

        private class GooglePolyline
        {
            [JsonPropertyName("encodedPolyline")]
            public string? EncodedPolyline { get; set; }
        }
    }
}
