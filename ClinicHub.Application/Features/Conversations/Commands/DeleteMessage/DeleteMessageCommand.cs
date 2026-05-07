using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Commands.DeleteMessage
{
    public record DeleteMessageCommand(Guid MessageId) : IRequest<string>;

    public class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
    {
        public DeleteMessageCommandValidator()
        {
            RuleFor(x => x.MessageId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));
        }
    }
}
