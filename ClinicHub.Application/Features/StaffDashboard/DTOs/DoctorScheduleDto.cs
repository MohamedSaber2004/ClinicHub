namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class DoctorScheduleDto
    {
        public DoctorBriefDto Doctor { get; set; } = null!;
        public string Date { get; set; } = null!;
        public List<DoctorSlotDto> Appointments { get; set; } = new();
    }
}
