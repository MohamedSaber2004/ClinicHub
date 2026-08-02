namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class StaffAppointmentDto
    {
        public Guid Id { get; set; }
        public PatientBriefDto Patient { get; set; } = null!;
        public DoctorBriefDto Doctor { get; set; } = null!;
        public string Specialty { get; set; } = null!;
        public string Date { get; set; } = null!;
        public string Time { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string StatusLabel { get; set; } = null!;
        public string StatusClass { get; set; } = null!;
    }
}
