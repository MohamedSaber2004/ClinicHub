namespace ClinicHub.Application.Common.Options
{
    /// <summary>
    /// Configuration settings for data seeding.
    /// </summary>
    public class SeedingSettings
    {
        public bool Enabled { get; set; }
        public int UserCount { get; set; } = 5;
        public int PostCount { get; set; } = 20;
        public int CommentsPerPost { get; set; } = 3;
        public int ReactionsPerPost { get; set; } = 5;
        public int? DoctorCount { get; set; } = 5;
        public int? DoctorAvailabilityCount { get; set; } = 5;
        public int? AppointmentCount { get; set; } = 20;
        public int? SupportTicketCount { get; set; } = 10;
        public int? SubscriptionCount { get; set; } = 5;
        public int? AdvertisementCount { get; set; } = 5;
        public int? AuditLogCount { get; set; } = 20;
    }
}
