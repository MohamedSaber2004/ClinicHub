namespace ClinicHub.Infrastructure.Services.Interfaces
{
    public interface IMapService
    {
        Task<List<ClinicExternalDto>> GetNearbyFromMapAsync(double lat, double lng, string category, CancellationToken cancellationToken, double radius = 5000);
        Task<List<ClinicExternalDto>> GeocodeAsync(string address, CancellationToken cancellationToken, int limit = 10);
        Task<string?> ReverseGeocodeAsync(double lat, double lng, CancellationToken cancellationToken);
        Task<RouteDto?> GetRouteAsync(double startLat, double startLng, double endLat, double endLng, CancellationToken cancellationToken);
    }

    public class RouteDto
    {
        public double Distance { get; set; } // meters
        public double Duration { get; set; } // seconds
        public List<List<double>>? Geometry { get; set; } // [[lng, lat], ...]
    }

    public class ClinicExternalDto
    {
        public string Name { get; set; } = null!;
        public string? NameAr { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? Address { get; set; }
        public string? AddressAr { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
    }
}
