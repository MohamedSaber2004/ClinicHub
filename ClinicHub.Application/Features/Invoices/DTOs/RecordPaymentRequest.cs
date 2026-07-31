using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Invoices.DTOs;

public class RecordPaymentRequest
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethodType Method { get; set; }
    public string? TransactionRef { get; set; }
    public string? Notes { get; set; }
}
