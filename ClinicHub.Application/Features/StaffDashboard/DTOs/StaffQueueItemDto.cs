using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class StaffQueueItemDto
    {
        public Guid AppointmentId { get; set; }
        public string PatientFullName { get; set; } = null!;
        public string? DoctorName { get; set; }
        public string StartTime { get; set; } = null!;
        public AppointmentStatus Status { get; set; }
        public int WaitTimeMinutes { get; set; }
    }
}
