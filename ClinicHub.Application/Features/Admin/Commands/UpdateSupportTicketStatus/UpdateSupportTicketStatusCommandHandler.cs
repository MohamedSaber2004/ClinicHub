using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.UpdateSupportTicketStatus
{
    public class UpdateSupportTicketStatusCommandHandler : IRequestHandler<UpdateSupportTicketStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSupportTicketStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateSupportTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.GetRepository<SupportTicket, Guid>().GetByIdAsync(request.TicketId);

            ticket.Status = request.Status;

            if (request.Status == SupportTicketStatus.Resolved || request.Status == SupportTicketStatus.Closed)
                ticket.ResolvedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<SupportTicket, Guid>().Update(ticket);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
