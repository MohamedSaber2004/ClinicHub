using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.Users.Commands.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator(
            IStringLocalizer<Messages> localizer,
            IOptions<IdentityModel> identityOptions)
        {
            var identityConfig = identityOptions.Value;

            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .When(x => x.Id is null);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MinimumLength(identityConfig.RequiredLength)
                    .WithMessage(localizer[LocalizationKeys.ValidationMessages.MinLength.Value])
                .Must(password => password.Any(char.IsUpper))
                    .WithMessage(localizer[LocalizationKeys.AuthMessages.WeakPassword.Value])
                .Must(password => password.Any(char.IsDigit))
                    .WithMessage(localizer[LocalizationKeys.AuthMessages.WeakPassword.Value]);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                    .WithMessage(localizer[LocalizationKeys.AuthMessages.PasswordMismatch.Value]);
        }
    }
}
