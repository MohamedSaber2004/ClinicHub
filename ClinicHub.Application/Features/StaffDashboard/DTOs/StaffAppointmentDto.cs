using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class StaffAppointmentDto
    {
        public Guid Id { get; set; }
        public string? DoctorName { get; set; }
        public string? BookedByUserName { get; set; }
        public string AppointmentDate { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public AppointmentType AppointmentType { get; set; }
        public AppointmentStatus Status { get; set; }
        public string PatientFullName { get; set; } = null!;
        public string PatientPhoneNumber { get; set; } = null!;
        public int PatientAge { get; set; }
        public Gender PatientGender { get; set; }
        public string Complaint { get; set; } = null!;
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
