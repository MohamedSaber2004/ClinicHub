namespace ClinicHub.Application.Common.Options
{
    public class GoogleMapsSettings
    {
        public string NearByFromMapBaseUrl { get; set; } = null!;
        public string GeoCodeBaseUrl { get; set; } = null!;
        public string RoutesBaseUrl { get; set; } = null!;
        public string ApiKey {  get; set; } = null!;
    }
}
