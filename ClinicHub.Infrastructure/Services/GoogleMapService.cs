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

        public async Task<List<ClinicExternalDto>> GetNearbyFromMapAsync(double lat, double lng, string category, CancellationToken cancellationToken, double radius = 5000)
        {
            var cacheKey = $"Google_Nearby_{lat}_{lng}_{category}_{radius}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            var googleKeyword = category.Contains(",") ? category.Replace(",", " ") : category;
            var url = $"{_options.NearByFromMapBaseUrl}/json?location={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}&radius={radius}&keyword={Uri.EscapeDataString(googleKeyword)}&key={_options.ApiKey}";
            
            try
            {
                var response = await _httpClient.GetFromJsonAsync<GooglePlacesResponse>(url, cancellationToken);
                
                if (response?.Status != "OK" && response?.Status != "ZERO_RESULTS")
                {
                    _logger.LogError("Google Maps API Error: Status={Status}, Message={Message}", response?.Status, response?.ErrorMessage);
                }

                if (response?.Results is not null && response.Results.Any())
                {
                    var results = response.Results.Select(r => new ClinicExternalDto
                    {
                        Name = r.Name ?? "Unknown Clinic",
                        Lat = r.Geometry?.Location?.Lat ?? 0,
                        Lng = r.Geometry?.Location?.Lng ?? 0,
                        Address = r.Vicinity,
                        Phone = null, // Needs individual Place Details call for real phone
                        Website = null
                    }).ToList();

                    _cache.Set(cacheKey, results, TimeSpan.FromMinutes(30));
                    return results;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GoogleMapService.GetNearbyFromMapAsync");
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
                
                if (response?.Status != "OK" && response?.Status != "ZERO_RESULTS")
                {
                    _logger.LogError("Google Geocoding API Error: Status={Status}, Message={Message}", response?.Status, response?.ErrorMessage);
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GoogleMapService.GeocodeAsync");
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
            var url = $"{_options.RoutesBaseUrl}/json?origin={startLat.ToString(CultureInfo.InvariantCulture)},{startLng.ToString(CultureInfo.InvariantCulture)}&destination={endLat.ToString(CultureInfo.InvariantCulture)},{endLng.ToString(CultureInfo.InvariantCulture)}&key={_options.ApiKey}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoogleDirectionsResponse>(url, cancellationToken);

                var route = response?.Routes?.FirstOrDefault();
                var leg = route?.Legs?.FirstOrDefault();
                if (leg == null) return null;

                return new RouteDto
                {
                    Distance = leg.Distance?.Value ?? 0,
                    Duration = (leg.Duration?.Value ?? 0) / 60,
                    Geometry = null 
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private class GooglePlacesResponse
        {
            public List<GooglePlaceResult>? Results { get; set; }
            public string? Status { get; set; }
            [JsonPropertyName("error_message")]
            public string? ErrorMessage { get; set; }
        }

        private class GooglePlaceResult
        {
            public string? Name { get; set; }
            public string? Vicinity { get; set; }
            public GoogleGeometry? Geometry { get; set; }
            public double? Rating { get; set; }
            public List<GooglePhoto>? Photos { get; set; }
        }

        private class GooglePhoto
        {
            [JsonPropertyName("photo_reference")]
            public string? PhotoReference { get; set; }
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

        private class GoogleDirectionsResponse
        {
            public List<GoogleDirectionRoute>? Routes { get; set; }
            public string? Status { get; set; }
            [JsonPropertyName("error_message")]
            public string? ErrorMessage { get; set; }
        }

        private class GoogleDirectionRoute
        {
            public List<GoogleDirectionLeg>? Legs { get; set; }
        }

        private class GoogleDirectionLeg
        {
            public GoogleValueObject? Distance { get; set; }
            public GoogleValueObject? Duration { get; set; }
        }

        private class GoogleValueObject
        {
            public double Value { get; set; }
            public string? Text { get; set; }
        }
    }
}
