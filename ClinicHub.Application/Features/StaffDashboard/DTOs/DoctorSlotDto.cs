namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class DoctorSlotDto
    {
        public PatientBriefDto Patient { get; set; } = null!;
        public string Time { get; set; } = null!;
        public string StatusLabel { get; set; } = null!;
        public string StatusClass { get; set; } = null!;
    }
}
