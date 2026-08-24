using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.ClinicPayments.DTOs;

public class AppointmentPaymentDto
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public DateTime AppointmentDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
}
