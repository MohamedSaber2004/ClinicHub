using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Commands.CreateConversation
{
    public record CreateConversationCommand(Guid RecipientId) : IRequest<Guid>;

    public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationCommandValidator()
        {
            RuleFor(x => x.RecipientId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));
        }
    }
}
