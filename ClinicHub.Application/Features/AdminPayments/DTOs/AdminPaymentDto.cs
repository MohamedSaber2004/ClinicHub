using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayments.DTOs;

public class AdminPaymentDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public PaymentType Type { get; set; }
    public string Payer { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime Date { get; set; }
    public string? RefNumber { get; set; }
}
