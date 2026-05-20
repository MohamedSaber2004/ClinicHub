using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.DeliverPendingMessages
{
    public class DeliverPendingMessagesCommandHandler : IRequestHandler<DeliverPendingMessagesCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPusherService _pusherService;
        private readonly ICurrentUserService _currentUserService;

        public DeliverPendingMessagesCommandHandler(IUnitOfWork unitOfWork, IPusherService pusherService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _pusherService = pusherService;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeliverPendingMessagesCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var conversations = await _unitOfWork.ConversationRepository.GetConversationsByUserIdAsync(userId, cancellationToken);
            bool hasChanges = false;

            foreach (var conversation in conversations)
            {
                var undeliveredMessages = conversation.Messages
                    .Where(m => m.SenderId != userId &&
                                m.Status != MessageStatus.Read &&
                                m.Status != MessageStatus.Delivered)
                    .ToList();

                if (undeliveredMessages.Any())
                {
                    foreach (var message in undeliveredMessages)
                    {
                        message.MarkAsDelivered();
                    }

                    hasChanges = true;

                    // The sender is the other user in the conversation
                    var senderId = conversation.InitiatorId == userId ? conversation.RecipientId : conversation.InitiatorId;

                    // Trigger messages-delivered event to the sender
                    await _pusherService.TriggerEventAsync(
                        $"private-user-{senderId}",
                        "messages-delivered",
                        new { conversationId = conversation.Id }
                    );
                }
            }

            if (hasChanges)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }
    }
}
