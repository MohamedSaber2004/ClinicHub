using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetUrgentSupportTickets
{
    public record GetUrgentSupportTicketsQuery : IRequest<List<SupportTicketDto>>;
}
