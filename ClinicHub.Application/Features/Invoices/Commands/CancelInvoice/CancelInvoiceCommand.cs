using ClinicHub.Application.Features.Invoices.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Commands.CancelInvoice;

public class CancelInvoiceCommand : IRequest<InvoiceDto>
{
    public Guid InvoiceId { get; set; }
    public string? Reason { get; set; }
}
