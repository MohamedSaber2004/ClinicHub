namespace ClinicHub.Application.Common.Models
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
    }
}
