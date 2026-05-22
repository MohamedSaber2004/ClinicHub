using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.DeleteConversation
{
    public record DeleteConversationCommand(Guid ConversationId) : IRequest<string>;

    public class DeleteConversationCommandValidator : AbstractValidator<DeleteConversationCommand>
    {
        public DeleteConversationCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
        }
    }
}
