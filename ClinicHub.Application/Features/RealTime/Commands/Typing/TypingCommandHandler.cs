using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.Typing
{
    public class TypingCommandHandler : IRequestHandler<TypingCommand, bool>
    {
        private readonly IPusherService _pusherService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public TypingCommandHandler(
            IPusherService pusherService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IStringLocalizer<Messages> localizer)
        {
            _pusherService = pusherService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            this._localizer = localizer;
        }

        public async Task<bool> Handle(TypingCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(request.ConversationId);
            if (conversation == null)
                throw new NotFoundException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.RealTimeMessages.ConversationNotFound.Value]));

            if (conversation.InitiatorId != currentUserId && conversation.RecipientId != currentUserId)
                throw new UnAuthorizedException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.RealTimeMessages.NotConversationParticipant]));

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
