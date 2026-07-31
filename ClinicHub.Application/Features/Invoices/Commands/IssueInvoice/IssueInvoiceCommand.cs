using ClinicHub.Application.Features.Invoices.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Commands.IssueInvoice;

public class IssueInvoiceCommand : IRequest<InvoiceDto>
{
    public Guid InvoiceId { get; set; }
}
