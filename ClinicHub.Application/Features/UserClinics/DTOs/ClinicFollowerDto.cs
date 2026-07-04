namespace ClinicHub.Application.Features.UserClinics.DTOs
{
    public class ClinicFollowerDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}
