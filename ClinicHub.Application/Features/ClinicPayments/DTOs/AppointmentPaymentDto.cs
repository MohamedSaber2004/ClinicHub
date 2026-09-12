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
    /// <summary>Superadmin platform-fee cut backed out of <see cref="Amount"/>.</summary>
    public decimal PlatformFee { get; set; }
    /// <summary>Clinic net for this appointment: <see cref="Amount"/> minus <see cref="PlatformFee"/>.</summary>
    public decimal ClinicNetAmount { get; set; }
}
