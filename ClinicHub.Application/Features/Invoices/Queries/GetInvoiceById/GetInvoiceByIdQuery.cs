using ClinicHub.Application.Features.Invoices.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQuery : IRequest<InvoiceDto>
{
    public Guid InvoiceId { get; set; }
}
