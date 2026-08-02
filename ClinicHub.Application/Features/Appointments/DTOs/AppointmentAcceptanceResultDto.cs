using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Appointments.DTOs;

/// <summary>
/// Returned by every accept path (staff approve, doctor accept / status=6).
/// Carries the created payment + Paymob hosted checkout link so the mobile app
/// can direct the patient to payment.
/// </summary>
public class AppointmentAcceptanceResultDto
{
    public Guid AppointmentId { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Accepted;
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? PaymobRedirectUrl { get; set; }
    public string? PaymobPaymentKey { get; set; }
}
