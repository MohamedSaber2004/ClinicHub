using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.Typing
{
    public class TypingCommand : IRequest<bool>
    {
        public Guid ConversationId { get; set; }
        public bool IsTyping { get; set; }
    }

    public class TypingCommandHandler : IRequestHandler<TypingCommand, bool>
    {
        private readonly IPusherService _pusherService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public TypingCommandHandler(
            IPusherService pusherService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _pusherService = pusherService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(TypingCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(request.ConversationId);
            if (conversation == null)
                throw new NotFoundException(LocalizationKeys.RealTimeMessages.ConversationNotFound);

            if (conversation.InitiatorId != currentUserId && conversation.RecipientId != currentUserId)
                throw new UnAuthorizedException(LocalizationKeys.RealTimeMessages.NotConversationParticipant);

            var recipientId = conversation.InitiatorId == currentUserId
                ? conversation.RecipientId
                : conversation.InitiatorId;

            await _pusherService.TriggerEventAsync($"private-user-{recipientId}", "typing", new
            {
                conversationId = request.ConversationId,
                isTyping = request.IsTyping,
                userId = currentUserId
            });

            return true;
        }
    }
}
