using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.UpdateSupportTicketStatus
{
    public class UpdateSupportTicketStatusCommand : IRequest<bool>
    {
        public Guid TicketId { get; set; }
        public SupportTicketStatus Status { get; set; }
    }
}
