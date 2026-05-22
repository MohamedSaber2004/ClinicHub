using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.UpdateConversation
{
    public class UpdateConversationCommandValidator : AbstractValidator<UpdateConversationCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateConversationCommandValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Key]))
                .MustAsync(ConversationExists)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.ConversationNotFound.Key]));

            RuleFor(x => x.Name)
                .MaximumLength(255)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Key]))
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.GroupPhotoUrl)
                .MaximumLength(500)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Key]))
                .When(x => !string.IsNullOrWhiteSpace(x.GroupPhotoUrl));

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.GroupPhotoUrl))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.AtLeastOneFieldRequired.Key]));
        }

        private async Task<bool> ConversationExists(Guid conversationId, CancellationToken cancellationToken)
        {
            return await _ctx.ConversationRepository.ExistsAsync(c => c.Id == conversationId, cancellationToken);
        }
    }
}
