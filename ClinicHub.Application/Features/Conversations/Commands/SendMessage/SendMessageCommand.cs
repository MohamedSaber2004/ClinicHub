using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Commands.SendMessage
{
    public record SendMessageCommand(Guid ConversationId, string Content) : IRequest<Guid>;

    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MaximumLength(5000)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value));
        }
    }
}
