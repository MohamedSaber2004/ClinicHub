namespace ClinicHub.Application.Common.Models
{
    public class IdentityModel
    {
        public bool RequiredDigit { get; set; }
        public int RequiredLength { get; set; }
        public bool RequireLowercase { get; set; }
        public int RequiredUniqueChars { get; set; }
        public bool RequireUppercase { get; set; }
        public int MaxFailedAttempts { get; set; }
        public int LockoutTimeSpanInDays { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public string AllowedUserNameCharacters { get; set; } = null!;
        public bool RequireUniqueEmail { get; set; }
        public bool RequireConfirmedEmail { get; set; }
    }
}
