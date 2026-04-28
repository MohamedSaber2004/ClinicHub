using ClinicHub.Application.Features.Auth.Commands.ForgetPassword;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordCommandValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidEmail.Value));

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .Length(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.ResetTokenInvalid.Value))
                .Matches(@"^\d{6}$").WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.ResetTokenInvalid.Value));

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MinimumLength(8).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MinLength.Value))
                .Matches(@"[A-Z]").WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value))
                .Matches(@"[0-9]").WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .Equal(x => x.NewPassword).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.PasswordMismatch.Value));

            RuleFor(x => x)
                .MustAsync(BeValidOtp).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.ResetTokenInvalid.Value));
        }

        private async Task<bool> BeValidOtp(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow) return false;

            var submittedHash = ForgetPasswordCommandHandler.ComputeSha256Hash(request.Token);
            return string.Equals(user.PasswordResetToken, submittedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
