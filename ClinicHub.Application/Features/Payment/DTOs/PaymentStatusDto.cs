using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Payment.DTOs;

public class PaymentStatusDto
{
    public Guid PaymentId { get; set; }
    public Guid AppointmentId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
}