using ClinicHub.Infrastructure.Services.Interfaces;
using System.Net.Http.Json;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services
{
    public class NominatimService : IMapService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NominatimService> _logger;

        public NominatimService(HttpClient httpClient, IMemoryCache cache, ILogger<NominatimService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Global timeout high enough, but we use CTS per request
        }

        public async Task<List<ClinicExternalDto>> GetNearbyFromMapAsync(double lat, double lng, string category, CancellationToken cancellationToken, double radius = 5000)
        {
            var cacheKey = $"Nearby_{lat}_{lng}_{category}_{radius}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            var tagsList = category.Replace(",", "|");
            var query = $@"
                [out:json][timeout:25];
                (
                  node(around:{radius},{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)})[""amenity""~""{tagsList}""];
                  way(around:{radius},{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)})[""amenity""~""{tagsList}""];
                  relation(around:{radius},{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)})[""amenity""~""{tagsList}""];
                );
                out center;";
            var url = $"https://overpass-api.de/api/interpreter?data={Uri.EscapeDataString(query)}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<OverpassResponse>(url, cancellationToken);
                if (response?.Elements == null) return new List<ClinicExternalDto>();

                var results = response.Elements.Select(e => {
                    return new ClinicExternalDto
                    {
                        Name = e.Tags?.GetValueOrDefault("name") ?? e.Tags?.GetValueOrDefault("name:en") ?? "Unknown Clinic",
                        NameAr = e.Tags?.GetValueOrDefault("name:ar"),
                        Lat = e.Lat != 0 ? e.Lat : (e.Center?.Lat ?? 0),
                        Lng = e.Lon != 0 ? e.Lon : (e.Center?.Lon ?? 0),
                        Address = e.Tags?.GetValueOrDefault("addr:full") ?? e.Tags?.GetValueOrDefault("addr:street") ?? "Near " + (e.Tags?.GetValueOrDefault("addr:city") ?? ""),
                        Phone = e.Tags?.GetValueOrDefault("phone") ?? e.Tags?.GetValueOrDefault("contact:phone"),
                        Website = e.Tags?.GetValueOrDefault("website") ?? e.Tags?.GetValueOrDefault("contact:website")
                    };
                }).ToList();

                _cache.Set(cacheKey, results, TimeSpan.FromMinutes(30));
                return results;
            }
            catch (Exception)
            {
                return new List<ClinicExternalDto>();
            }
        }

        public async Task<List<ClinicExternalDto>> GeocodeAsync(string address, CancellationToken cancellationToken, int limit = 10)
        {
            var cacheKey = $"Geocode_{address}_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<ClinicExternalDto>? cachedResults))
            {
                return cachedResults ?? new List<ClinicExternalDto>();
            }

            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit={limit}&countrycodes=eg";
            try
            {
                var results = await _httpClient.GetFromJsonAsync<NominatimResult[]>(url, cancellationToken);
                if (results != null && results.Length > 0)
                {
                    var mappedResults = results.Select(res => new ClinicExternalDto
                    {
                        Name = res.Display_Name?.Split(',')[0] ?? "Unknown",
                        Lat = double.Parse(res.Lat ?? "0", CultureInfo.InvariantCulture),
                        Lng = double.Parse(res.Lon ?? "0", CultureInfo.InvariantCulture),
                        Address = res.Display_Name
                    }).ToList();

                    _cache.Set(cacheKey, mappedResults, TimeSpan.FromHours(24));
                    return mappedResults;
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
            var cacheKey = $"Route_{startLat}_{startLng}_{endLat}_{endLng}";
            if (_cache.TryGetValue(cacheKey, out RouteDto? cachedRoute))
            {
                return cachedRoute;
            }

            var endpoints = new[]
            {
                "https://routing.openstreetmap.de/routed-car",
                "http://routing.openstreetmap.de/routed-car",
                "http://router.project-osrm.org",
                "https://routing.openstreetmap.fr/routed-car"
            };

            foreach (var baseUrl in endpoints)
            {
                var url = $"{baseUrl}/route/v1/driving/{startLng.ToString(CultureInfo.InvariantCulture)},{startLat.ToString(CultureInfo.InvariantCulture)};{endLng.ToString(CultureInfo.InvariantCulture)},{endLat.ToString(CultureInfo.InvariantCulture)}?overview=full&geometries=geojson";
                
                try
                {
                    _logger.LogInformation("Attempting to fetch route from: {Url}", url);
                    
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(3)); // 3 seconds per attempt

                    var response = await _httpClient.GetFromJsonAsync<OsrmResponse>(url, cts.Token);
                    var route = response?.Routes?.FirstOrDefault();
                    
                    if (route != null)
                    {
                        var result = new RouteDto
                        {
                            Distance = route.Distance,
                            Duration = route.Duration / 60,
                            Geometry = route.Geometry?.Coordinates
                        };

                        _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Routing attempt failed for {BaseUrl}: {Message}", baseUrl, ex.Message);
                }
            }

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
