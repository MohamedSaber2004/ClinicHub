using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.RegisterFcmToken
{
    public class RegisterFcmTokenCommandValidator : AbstractValidator<RegisterFcmTokenCommand>
    {
        public RegisterFcmTokenCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.FcmToken)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.FcmTokenRequired.Value]));

            RuleFor(x => x.DevicePlatform)
                .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.FcmTokenPlatformRequired.Value]));
        }
    }
}
