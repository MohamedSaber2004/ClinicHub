using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Commands.RecordPayment;

public class RecordPaymentCommand : IRequest<Guid>
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethodType Method { get; set; }
    public string? TransactionRef { get; set; }
    public string? Notes { get; set; }
}
