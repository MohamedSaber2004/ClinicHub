namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class DoctorBriefDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Specialty { get; set; } = null!;
    }
}
