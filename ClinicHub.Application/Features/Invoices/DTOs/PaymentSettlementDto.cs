using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Invoices.DTOs;

public class PaymentSettlementDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethodType Method { get; set; }
    public string? TransactionRef { get; set; }
    public string? PaymobPaymentKey { get; set; }
    public string Status { get; set; } = "Completed";
    public decimal? RefundedAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime PaidAt { get; set; }
}
