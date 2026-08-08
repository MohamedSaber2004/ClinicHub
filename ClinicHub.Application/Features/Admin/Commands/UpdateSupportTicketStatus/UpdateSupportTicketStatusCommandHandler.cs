using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.Admin.Commands.UpdateSupportTicketStatus
{
    public class UpdateSupportTicketStatusCommandHandler : IRequestHandler<UpdateSupportTicketStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFcmService _fcmService;
        private readonly ILogger<UpdateSupportTicketStatusCommandHandler> _logger;

        public UpdateSupportTicketStatusCommandHandler(IUnitOfWork unitOfWork, IFcmService fcmService, ILogger<UpdateSupportTicketStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateSupportTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.GetRepository<SupportTicket, Guid>().GetByIdAsync(request.TicketId);

            ticket.Status = request.Status;

            if (request.Status == SupportTicketStatus.Resolved || request.Status == SupportTicketStatus.Closed)
                ticket.ResolvedAt = DateTime.Now;

            _unitOfWork.GetRepository<SupportTicket, Guid>().Update(ticket);

            await NotifyTicketOwnerAsync(ticket);

            // Single commit: the ticket update and the notification row in one transaction.
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task NotifyTicketOwnerAsync(SupportTicket ticket)
        {
            try
            {
                await _fcmService.SendToUserAsync(ticket.UserId, NotificationType.SupportTicketUpdate, new()
                {
                    ["ticketId"] = ticket.Id.ToString(),
                    ["subject"] = ticket.Subject,
                    ["status"] = ticket.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send support-ticket notification for ticket {TicketId}.", ticket.Id);
            }
        }
    }
}
