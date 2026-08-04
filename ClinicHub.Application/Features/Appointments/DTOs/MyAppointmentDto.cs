using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Appointments.DTOs;

/// <summary>
/// Patient-facing appointment payload used by the mobile "My appointments" screen.
/// </summary>
public class MyAppointmentDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string? ClinicName { get; set; }
    public Guid DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string Date { get; set; } = null!;
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public string Status { get; set; }
    public string? RejectionReason { get; set; }
    public MyAppointmentPaymentDto? Payment { get; set; }
}

public class MyAppointmentPaymentDto
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentStatus PaymentStatus { get; set; }
    /// <summary>Present only while the appointment is accepted &amp; unpaid (status = 6).</summary>
    public string? PaymobRedirectUrl { get; set; }
}
