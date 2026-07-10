using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.Auth.Commands.Signup
{
    public class SignupCommandValidator : AbstractValidator<SignupCommand>
    {
        public SignupCommandValidator(
            IStringLocalizer<Messages> localizer,
            UserManager<Domain.Entities.ApplicationUser> userManager,
            IOptions<IdentityModel> identityOptions)
        {
            var identityConfig = identityOptions.Value;

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(async (email, ct) =>
                {
                    var user = await userManager.FindByEmailAsync(email);
                    return user is null || user.IsDeleted;
                }).WithMessage(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MinimumLength(identityConfig.RequiredLength).WithMessage(localizer[LocalizationKeys.ValidationMessages.MinLength.Value])
                .Must(password => password.Any(char.IsUpper)).WithMessage(localizer[LocalizationKeys.AuthMessages.WeakPassword.Value])
                .Must(password => password.Any(char.IsDigit)).WithMessage(localizer[LocalizationKeys.AuthMessages.WeakPassword.Value]);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(localizer[LocalizationKeys.AuthMessages.PasswordMismatch.Value]);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(20).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^1[0125]\d{8}$").WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(async (phone, ct) =>
                {
                    var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && !u.IsDeleted, ct);
                    return user is null;
                }).WithMessage(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value]);

            RuleFor(x => x.BirthDate)
                .Must(date => date == null || date <= DateTime.Today.AddYears(-15))
                .WithMessage(localizer[LocalizationKeys.ValidationMessages.MinAge.Value]);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value])
                .When(x => x.Gender.HasValue);

            RuleFor(x => x.TypeOfUser)
                .IsInEnum().WithMessage(localizer[LocalizationKeys.AuthMessages.InvalidUserType.Value]);
        }
    }
}
