using ClinicHub.Infrastructure.Services.Interfaces;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json.Serialization;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicHub.Infrastructure.Services
{
    public class GoogleMapService : IMapService
    {
        private const string NearbyFieldMask = "places.displayName,places.formattedAddress,places.location,places.rating";
        private const string RouteFieldMask = "routes.distanceMeters,routes.duration,routes.polyline.encodedPolyline";

        private readonly HttpClient _httpClient;
        private readonly GoogleMapsSettings _options;
        private readonly IMemoryCache _cache;

        public GoogleMapService(HttpClient httpClient, IOptions<GoogleMapsSettings> options, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _cache = cache;
        }

        public async Task<List<ClinicExternalDto>> GetNearbyFromMapAsync(double lat, double lng, string category, CancellationToken cancellationToken, double radius = 5000)
        {
            var cacheKey = $"Google_Nearby_{lat}_{lng}_{category}_{radius}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            var includedTypes = NormalizePlaceTypes(category).ToList();
            if (includedTypes.Count == 0)
            {
                includedTypes = ["hospital"];
            }

            var requestBody = new GoogleNearbySearchRequest
            {
                IncludedTypes = includedTypes,
                LanguageCode = "en",
                MaxResultCount = 20,
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

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _options.NearByFromMapBaseUrl);
                request.Headers.Add("X-Goog-Api-Key", _options.ApiKey);
                request.Headers.Add("X-Goog-FieldMask", NearbyFieldMask);
                request.Content = JsonContent.Create(requestBody);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var payload = await response.Content.ReadFromJsonAsync<GoogleNearbySearchResponse>(cancellationToken: cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<ClinicExternalDto>();
                }

                if (payload?.Places is not null && payload.Places.Any())
                {
                    var results = payload.Places.Select(r => new ClinicExternalDto
                    {
                        Name = r.DisplayName?.Text ?? "Unknown Clinic",
                        Lat = r.Location?.Latitude ?? 0,
                        Lng = r.Location?.Longitude ?? 0,
                        Address = r.FormattedAddress,
                        Phone = null,
                        Website = null
                    }).ToList();

                    _cache.Set(cacheKey, results, TimeSpan.FromMinutes(30));
                    return results;
                }
            }
            catch
            {
            }

            return new List<ClinicExternalDto>();
        }

        public async Task<List<ClinicExternalDto>> GeocodeAsync(string address, CancellationToken cancellationToken, int limit = 10)
        {
            var cacheKey = $"Google_Geocode_{address}_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            var url = $"{_options.GeoCodeBaseUrl}/json?address={Uri.EscapeDataString(address)}&components=country:EG&key={_options.ApiKey}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(url, cancellationToken);

                if (response?.Results != null && response.Results.Any())
                {
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
            }
            catch
            {
            }

            return new List<ClinicExternalDto>();
        }

        public async Task<string?> ReverseGeocodeAsync(double lat, double lng, CancellationToken cancellationToken)
        {
            var url = $"{_options.GeoCodeBaseUrl}/json?latlng={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}&key={_options.ApiKey}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(url, cancellationToken);
                return response?.Results?.FirstOrDefault()?.FormattedAddress;
            }
            catch { }
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
                    return null;
                }

                var route = payload?.Routes?.FirstOrDefault();
                if (route is null)
                {
                    return null;
                }

                return new RouteDto
                {
                    Distance = route.DistanceMeters,
                    Duration = ParseDurationToMinutes(route.Duration),
                    Geometry = DecodePolyline(route.Polyline?.EncodedPolyline)
                };
            }
            catch
            {
                return null;
            }
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

            [JsonPropertyName("error")]
            public GoogleApiError? Error { get; set; }
        }

        private class GooglePlaceResult
        {
            [JsonPropertyName("displayName")]
            public GoogleDisplayName? DisplayName { get; set; }

            [JsonPropertyName("formattedAddress")]
            public string? FormattedAddress { get; set; }

            [JsonPropertyName("location")]
            public GoogleGoogleLocation? Location { get; set; }

            [JsonPropertyName("rating")]
            public double? Rating { get; set; }
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
