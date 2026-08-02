using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.DoctorDashboard.DTOs
{
    public class DoctorAppointmentDto
    {
        public Guid Id { get; set; }
        public Guid BookedByUserId { get; set; }
        public string? BookedByUserName { get; set; }
        public string? BookedByUserPhone { get; set; }
        public string AppointmentDate { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public AppointmentType AppointmentType { get; set; }
        public AppointmentStatus Status { get; set; }
        public string PatientFullName { get; set; } = null!;
        public string? PatientPhoneNumber { get; set; }
        public int PatientAge { get; set; }
        public Gender PatientGender { get; set; }
        public string Complaint { get; set; } = null!;
        public string? ChronicDiseases { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ClinicName { get; set; }
    }
}
