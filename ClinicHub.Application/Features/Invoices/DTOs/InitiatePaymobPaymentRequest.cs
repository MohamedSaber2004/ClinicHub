namespace ClinicHub.Application.Features.Invoices.DTOs;

public class InitiatePaymobPaymentRequest
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}
