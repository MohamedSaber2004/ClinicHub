using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;

namespace ClinicHub.Application.Features.Conversations.Commands.UpdateConversationParticipantSettings
{
    public class UpdateConversationParticipantSettingsCommandValidator : AbstractValidator<UpdateConversationParticipantSettingsCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateConversationParticipantSettingsCommandValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(ConversationExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.RealTimeMessages.ConversationNotFound.Value));

            // At least one setting should be provided
            RuleFor(x => x)
                .Must(x => x.IsMuted.HasValue || x.IsArchived.HasValue || x.IsBlocked.HasValue)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.AtLeastOneFieldRequired.Value));
        }

        private async Task<bool> ConversationExists(Guid conversationId, CancellationToken cancellationToken)
        {
            return await _ctx.ConversationRepository.ExistsAsync(c => c.Id == conversationId ,cancellationToken);
        }
    }
}
