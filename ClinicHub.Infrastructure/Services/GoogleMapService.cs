using ClinicHub.Infrastructure.Services.Interfaces;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json.Serialization;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace ClinicHub.Infrastructure.Services
{
    public class GoogleMapService : IMapService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleMapsSettings _options;

        public GoogleMapService(HttpClient httpClient,IOptions<GoogleMapsSettings> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<List<ClinicExternalDto>> GetNearbyFromMapAsync(double lat, double lng, string category, CancellationToken cancellationToken, double radius = 5000)
        {
            var url = $"{_options.NearByFromMapBaseUrl}/json?location={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}&radius={radius}&keyword={Uri.EscapeDataString(category)}&key={_options.ApiKey}";
            
            var response = await _httpClient.GetFromJsonAsync<GooglePlacesResponse>(url, cancellationToken);
                
            if (response?.Results is not null && response.Results.Any())
            {
                return response.Results.Select(r => new ClinicExternalDto
                {
                    Name = r.Name ?? "Unknown Clinic",
                    Lat = r.Geometry?.Location?.Lat ?? 0,
                    Lng = r.Geometry?.Location?.Lng ?? 0,
                    Address = r.Vicinity
                }).ToList();

            }

            return new List<ClinicExternalDto>();
        }

        public async Task<List<ClinicExternalDto>> GeocodeAsync(string address, CancellationToken cancellationToken, int limit = 10)
        {
            var url = $"{_options.GeoCodeBaseUrl}/json?address={Uri.EscapeDataString(address)}&components=country:EG&key={_options.ApiKey}";
                var response = await _httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(url, cancellationToken);

            if (response?.Results != null && response.Results.Any())
            {
                return response.Results.Take(limit).Select(res => new ClinicExternalDto
                {
                    Name = res.FormattedAddress?.Split(',')[0] ?? "Unknown",
                    Lat = res.Geometry?.Location?.Lat ?? 0,
                    Lng = res.Geometry?.Location?.Lng ?? 0,
                    Address = res.FormattedAddress
                }).ToList();
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
