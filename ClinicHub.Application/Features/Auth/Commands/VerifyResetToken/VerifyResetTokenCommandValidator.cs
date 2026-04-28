using ClinicHub.Application.Localization;
using FluentValidation;

namespace ClinicHub.Application.Features.Auth.Commands.VerifyResetToken
{
    public class VerifyResetTokenCommandValidator : AbstractValidator<VerifyResetTokenCommand>
    {
        public VerifyResetTokenCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidEmail.Value));

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .Length(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.ResetTokenInvalid.Value))
                .Matches(@"^\d{6}$").WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.ResetTokenInvalid.Value));
        }
    }
}
