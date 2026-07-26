namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class PatientBriefDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Initial { get; set; } = null!;
    }
}
