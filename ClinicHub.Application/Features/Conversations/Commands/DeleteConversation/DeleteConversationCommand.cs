using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Commands.DeleteConversation
{
    public record DeleteConversationCommand(Guid ConversationId) : IRequest<string>;

    public class DeleteConversationCommandValidator : AbstractValidator<DeleteConversationCommand>
    {
        public DeleteConversationCommandValidator()
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));
        }
    }
}
