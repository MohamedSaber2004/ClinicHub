using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.VerifyResetToken
{
    public class VerifyResetTokenCommandValidator : AbstractValidator<VerifyResetTokenCommand>
    {
        public VerifyResetTokenCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value]));
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .Length(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.ResetTokenInvalid.Value]))
                .Matches(@"^\d{6}$").WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.ResetTokenInvalid.Value]));
        }
    }
}
