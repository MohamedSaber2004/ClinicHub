using ClinicHub.Application.Localization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Conversations.Commands.DeleteMessage
{
    public record DeleteMessageCommand(Guid MessageId) : IRequest<string>;

    public class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
    {
        public DeleteMessageCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.MessageId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
        }
    }
}
