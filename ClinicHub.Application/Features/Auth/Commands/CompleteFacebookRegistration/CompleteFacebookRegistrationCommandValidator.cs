using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.CompleteFacebookRegistration
{
    public sealed class CompleteFacebookRegistrationCommandValidator : AbstractValidator<CompleteFacebookRegistrationCommand>
    {
        public CompleteFacebookRegistrationCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.FacebookTokenRequired.Value]));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.FacebookEmailRequired.Value]))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.InvalidEmail.Value]));
        }
    }
}
