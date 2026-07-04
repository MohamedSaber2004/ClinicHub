namespace ClinicHub.Application.Features.UserClinics.DTOs
{
    public class FollowedClinicDto
    {
        public Guid ClinicId { get; set; }
        public string Name { get; set; } = null!;
        public string? NameAr { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ImageUrl { get; set; }
        public double? Rating { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}
