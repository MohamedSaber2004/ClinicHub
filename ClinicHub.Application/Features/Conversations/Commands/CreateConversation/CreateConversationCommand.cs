using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.CreateConversation
{
    public record CreateConversationCommand(Guid RecipientId) : IRequest<Guid>;

    public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.RecipientId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
        }
    }
}
