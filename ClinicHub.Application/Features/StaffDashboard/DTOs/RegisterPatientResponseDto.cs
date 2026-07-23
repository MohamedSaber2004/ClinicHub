namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class RegisterPatientResponseDto
    {
        public Guid UserId { get; set; }
        public Guid AppointmentId { get; set; }
        public bool IsNewUser { get; set; }
    }
}
