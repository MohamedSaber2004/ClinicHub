namespace ClinicHub.Application.Common.Options
{
    public class FirebaseSettings
    {
        public string CredentialsFilePath { get; set; } = string.Empty;
        public PlatformNotificationSettings Web { get; set; } = new();
        public PlatformNotificationSettings Android { get; set; } = new();
        public PlatformNotificationSettings Ios { get; set; } = new();
    }

    public class PlatformNotificationSettings
    {
        public string? Sound { get; set; }
        public string? ClickAction { get; set; }
        public string? Icon { get; set; }
        public int? Badge { get; set; }
        public string? ChannelId { get; set; }
        public string? Category { get; set; }
        public string? Link { get; set; }
    }
}
