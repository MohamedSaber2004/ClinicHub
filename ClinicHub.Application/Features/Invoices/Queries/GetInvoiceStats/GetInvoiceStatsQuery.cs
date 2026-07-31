using ClinicHub.Application.Features.Invoices.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoiceStats;

public class GetInvoiceStatsQuery : IRequest<InvoiceStatsDto>
{
}
