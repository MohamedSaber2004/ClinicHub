using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;

namespace ClinicHub.Application.Features.Conversations.Commands.UpdateConversation
{
    public class UpdateConversationCommandValidator : AbstractValidator<UpdateConversationCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateConversationCommandValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(ConversationExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.RealTimeMessages.ConversationNotFound.Value));

            RuleFor(x => x.Name)
                .MaximumLength(255)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value))
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.GroupPhotoUrl)
                .MaximumLength(500)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value))
                .When(x => !string.IsNullOrWhiteSpace(x.GroupPhotoUrl));

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.GroupPhotoUrl))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.AtLeastOneFieldRequired.Value));
        }

        private async Task<bool> ConversationExists(Guid conversationId, CancellationToken cancellationToken)
        {
            return await _ctx.ConversationRepository.ExistsAsync(c => c.Id == conversationId, cancellationToken);
        }
    }
}
