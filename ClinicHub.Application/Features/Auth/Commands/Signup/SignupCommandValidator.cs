using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.Signup
{
    public class SignupCommandValidator : AbstractValidator<SignupCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SignupCommandValidator(UserManager<ApplicationUser> userManager, IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Key]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Key])
                .MustAsync(BeUniqueEmail).WithMessage(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Key]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                .MinimumLength(8).WithMessage(localizer[LocalizationKeys.ValidationMessages.MinLength.Key])
                .Matches(@"[A-Z]").WithMessage(localizer[LocalizationKeys.AuthMessages.WeakPassword.Key])
                .Matches(@"[0-9]").WithMessage(localizer[LocalizationKeys.AuthMessages.WeakPassword.Key]);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(localizer[LocalizationKeys.AuthMessages.PasswordMismatch.Key]);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                .MaximumLength(20).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Key])
                .Matches(@"^1[0125]\d{8}$").WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Key]);

            RuleFor(x => x.BirthDate)
                 .NotEmpty()
                 .WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                 .Must(BeAtLeast18)
                 .WithMessage(localizer[LocalizationKeys.ValidationMessages.MinAge.Key]);

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Key]);
        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null;
        }

        private bool BeAtLeast18(DateTime birthDate)
        {
            var minAllowedDate = DateTime.UtcNow.AddYears(-15);

            return birthDate <= minAllowedDate;
        }
    }
}
