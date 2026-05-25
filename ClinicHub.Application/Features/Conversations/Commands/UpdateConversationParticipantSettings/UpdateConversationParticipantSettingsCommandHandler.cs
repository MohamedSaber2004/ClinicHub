using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.UpdateConversationParticipantSettings
{
    public class UpdateConversationParticipantSettingsCommandHandler : IRequestHandler<UpdateConversationParticipantSettingsCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateConversationParticipantSettingsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<string> Handle(UpdateConversationParticipantSettingsCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            // Verify conversation exists
            var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(request.ConversationId);
            if (conversation == null)
                throw new NotFoundException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ValidationMessages.ConversationNotFound.Key]));

            // Get current user's participant record
            var participant = await _unitOfWork.ConversationParticipantRepository.GetParticipantAsync(request.ConversationId, currentUserId);
            if (participant == null)
                throw new BadRequestException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ValidationMessages.UnauthorizedAction.Key]));

            // Update settings
            if (request.IsMuted.HasValue)
                participant.ToggleMute(request.IsMuted.Value);

            if (request.IsArchived.HasValue)
                participant.ToggleArchive(request.IsArchived.Value);

            if (request.IsBlocked.HasValue)
                participant.ToggleBlock(request.IsBlocked.Value);

            _unitOfWork.ConversationParticipantRepository.Update(participant);
            await _unitOfWork.SaveChangesAsync();

            return JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ActionResults.Ok.Key]);
        }
    }
}
