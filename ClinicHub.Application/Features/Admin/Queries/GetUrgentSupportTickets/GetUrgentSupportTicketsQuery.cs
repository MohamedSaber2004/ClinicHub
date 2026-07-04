using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetUrgentSupportTickets
{
    public class GetUrgentSupportTicketsQuery : IRequest<PagginatedResult<SupportTicketDto>>
    {
        public int PageNumber { get; set; } = PagginatedResult<SupportTicketDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<SupportTicketDto>.DefaultPageSize;
    }
}
