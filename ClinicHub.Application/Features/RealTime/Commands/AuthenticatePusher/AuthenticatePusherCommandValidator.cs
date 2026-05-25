using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.AuthenticatePusher
{
    public class AuthenticatePusherCommandValidator : AbstractValidator<AuthenticatePusherCommand>
    {
        public AuthenticatePusherCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.SocketId)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.SocketIdRequired.Value]));

            RuleFor(x => x.ChannelName)
                .NotEmpty()
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.ChannelNameRequired.Value]));
        }
    }
}
