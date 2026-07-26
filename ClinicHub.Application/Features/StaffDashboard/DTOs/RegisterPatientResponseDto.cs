namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class RegisterPatientResponseDto
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public int QueueNumber { get; set; }
        public string Message { get; set; } = null!;
    }
}
