using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayments.DTOs;

public class AdminPaymentDetailDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public PaymentType Type { get; set; }
    public string Payer { get; set; } = string.Empty;
    public string PayerType { get; set; } = string.Empty;
    public string? PayerEmail { get; set; }
    public string? PayerPhone { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public string? RefNumber { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public List<PaymentTimelineEntryDto> Timeline { get; set; } = new();
}
