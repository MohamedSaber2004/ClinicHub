using ClinicHub.Infrastructure.Services.Interfaces;
using System.Net.Http.Json;
using System.Globalization;

namespace ClinicHub.Infrastructure.Services
{
    public class NominatimService : IMapService
    {
        private readonly HttpClient _httpClient;

        public NominatimService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ClinicExternalDto>> GetNearbyFromMapAsync(double lat, double lng, string category, CancellationToken cancellationToken, double radius = 5000)
        {
            var tags = category.Replace(",", "|");
            var query = $@"
                [out:json][timeout:5];
                (
                  node(around:{radius},{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)})[""amenity""~""{tags}""];
                  way(around:{radius},{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)})[""amenity""~""{tags}""];
                );
                out center;";
            var url = $"https://overpass-api.de/api/interpreter?data={Uri.EscapeDataString(query)}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<OverpassResponse>(url, cancellationToken);
                if (response?.Elements == null) return new List<ClinicExternalDto>();

                return response.Elements.Select(e => new ClinicExternalDto
                {
                    Name = e.Tags?.GetValueOrDefault("name") ?? e.Tags?.GetValueOrDefault("name:en") ?? "Unknown Clinic",
                    NameAr = e.Tags?.GetValueOrDefault("name:ar"),
                    Lat = e.Lat != 0 ? e.Lat : (e.Center?.Lat ?? 0),
                    Lng = e.Lon != 0 ? e.Lon : (e.Center?.Lon ?? 0),
                    Address = e.Tags?.GetValueOrDefault("addr:full") ?? e.Tags?.GetValueOrDefault("addr:street")
                }).ToList();
            }
            catch (Exception)
            {
                return new List<ClinicExternalDto>();
            }
        }

        public async Task<List<ClinicExternalDto>> GeocodeAsync(string address, CancellationToken cancellationToken, int limit = 10)
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit={limit}&countrycodes=eg";
            try
            {
                var results = await _httpClient.GetFromJsonAsync<NominatimResult[]>(url, cancellationToken);
                if (results != null && results.Length > 0)
                {
                    return results.Select(res => new ClinicExternalDto
                    {
                        Name = res.Display_Name?.Split(',')[0] ?? "Unknown",
                        Lat = double.Parse(res.Lat, CultureInfo.InvariantCulture),
                        Lng = double.Parse(res.Lon, CultureInfo.InvariantCulture),
                        Address = res.Display_Name
                    }).ToList();
                }
            }
            catch { }
            return new List<ClinicExternalDto>();
        }

        public async Task<string?> ReverseGeocodeAsync(double lat, double lng, CancellationToken cancellationToken)
        {
            var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lng.ToString(CultureInfo.InvariantCulture)}&format=json";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<NominatimReverseResult>(url, cancellationToken);
                return result?.Display_Name;
            }
            catch { }
            return null;
        }

        public async Task<RouteDto?> GetRouteAsync(double startLat, double startLng, double endLat, double endLng, CancellationToken cancellationToken)
        {
            var url = $"http://router.project-osrm.org/route/v1/driving/{startLng.ToString(CultureInfo.InvariantCulture)},{startLat.ToString(CultureInfo.InvariantCulture)};{endLng.ToString(CultureInfo.InvariantCulture)},{endLat.ToString(CultureInfo.InvariantCulture)}?overview=full&geometries=geojson";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<OsrmResponse>(url, cancellationToken);
                var route = response?.Routes?.FirstOrDefault();
                if (route == null) return null;

                return new RouteDto
                {
                    Distance = route.Distance,
                    Duration = route.Duration / 60,
                    Geometry = route.Geometry?.Coordinates
                };
            }
            catch { }
            return null;
        }

        private class NominatimResult
        {
            public string? Lat { get; set; }
            public string? Lon { get; set; }
            public string? Display_Name { get; set; }
        }

        private class NominatimReverseResult
        {
            public string? Display_Name { get; set; }
        }

        private class OverpassResponse
        {
            public List<OverpassElement>? Elements { get; set; }
        }

        private class OverpassElement
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
            public OverpassCenter? Center { get; set; }
            public Dictionary<string, string>? Tags { get; set; }
        }

        private class OverpassCenter
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        private class OsrmResponse
        {
            public List<OsrmRoute>? Routes { get; set; }
        }

        private class OsrmRoute
        {
            public double Distance { get; set; }
            public double Duration { get; set; }
            public OsrmGeometry? Geometry { get; set; }
        }

        private class OsrmGeometry
        {
            public List<List<double>>? Coordinates { get; set; }
        }
    }
}
