namespace ClinicHub.Application.Common.Options
{
    public class DeepLinkSettings
    {
        public string AppScheme { get; set; } = "clinichub";
        public string AndroidPackageName { get; set; } = "com.doctory";
        public string PlayStoreUrl { get; set; } = "https://play.google.com/store/apps/details?id=com.doctory";
        public string AppStoreUrl { get; set; } = "https://apps.apple.com/app/idYOUR_APPLE_APP_ID";
        public string? WebFallbackUrl { get; set; }
        public string AppNameAr { get; set; } = "كلينيك هب";
        public string AppNameEn { get; set; } = "ClinicHub";
    }
}
