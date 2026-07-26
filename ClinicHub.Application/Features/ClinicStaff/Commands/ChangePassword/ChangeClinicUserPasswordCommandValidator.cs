using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.ChangePassword
{
    public class ChangeClinicUserPasswordCommandValidator : AbstractValidator<ChangeClinicUserPasswordCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangeClinicUserPasswordCommandValidator(
            IStringLocalizer<Messages> localizer,
            IOptions<IdentityModel> identityOptions,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            var identityConfig = identityOptions.Value;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MinimumLength(identityConfig.RequiredLength)
                    .WithMessage(localizer[LocalizationKeys.ValidationMessages.MinLength.Value])
                .MustAsync(async (command, newPassword, ct) =>
                {
                    var user = await _userManager.FindByIdAsync(command.UserId.ToString());
                    if (user == null) return true;
                    return !await _userManager.CheckPasswordAsync(user, newPassword);
                }).WithMessage(localizer[LocalizationKeys.AuthMessages.PasswordSameAsOld.Value])
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
