using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.DisconnectUser
{
    public class DisconnectUserCommandValidator: AbstractValidator<DisconnectUserCommand>
    {
        public DisconnectUserCommandValidator(IStringLocalizer<Message> localizer)
        {
            RuleFor(x => x.ConnectionId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.ConnectionIdRequired.Value]));
        }
    }
}
