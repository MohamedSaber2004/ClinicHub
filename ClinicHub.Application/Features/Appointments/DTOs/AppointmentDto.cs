using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Appointments.DTOs
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid BookedByUserId { get; set; }
        public Guid DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public Guid ClinicId { get; set; }
        public string? ClinicName { get; set; }
        
        public string AppointmentDate { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;

        public AppointmentType AppointmentType { get; set; }
        public AppointmentStatus Status { get; set; }

        public string PatientFullName { get; set; } = null!;
        public int PatientAge { get; set; }
        public Gender PatientGender { get; set; }
        public string Complaint { get; set; } = null!;
        public string? ChronicDiseases { get; set; }
        
        public string? CancellationReason { get; set; }
        public string? BookingReference { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public Guid? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReceiptUrl { get; set; }
    }
}
