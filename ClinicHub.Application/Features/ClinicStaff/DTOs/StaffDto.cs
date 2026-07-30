namespace ClinicHub.Application.Features.ClinicStaff.DTOs
{
    public class StaffDto
    {
        public Guid Id { get; set; }
        public string? ImageName { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
