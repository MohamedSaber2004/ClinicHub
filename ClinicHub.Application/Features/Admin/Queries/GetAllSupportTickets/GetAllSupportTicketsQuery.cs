using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetAllSupportTickets
{
    public class GetAllSupportTicketsQuery : IRequest<PagginatedResult<SupportTicketDto>>
    {
        public SupportTicketStatus? Status { get; set; }
        public SupportTicketPriority? Priority { get; set; }
        public Guid? ClinicId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
