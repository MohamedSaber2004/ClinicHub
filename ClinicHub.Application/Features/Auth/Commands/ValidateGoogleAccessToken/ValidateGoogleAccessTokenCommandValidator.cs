using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.ValidateGoogleAccessToken
{
    public class ValidateGoogleAccessTokenCommandValidator: AbstractValidator<ValidateGoogleAccessTokenCommand>
    {
        public ValidateGoogleAccessTokenCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.GoogleTokenRequired.Value]));
        }
    }
}
