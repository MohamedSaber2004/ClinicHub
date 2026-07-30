using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.CreateStaff
{
    public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
    {
        public CreateStaffCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            RuleFor(v => v.FullName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(200).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value]))
                .MustAsync(async (email, ct) =>
                {
                    var user = await userManager.FindByEmailAsync(email);
                    return user == null;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.EmailAlreadyExists.Value]));

            RuleFor(v => v.PhoneNumber)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (phone, ct) =>
                {
                    var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone && !u.IsDeleted, ct);
                    return user is null;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.PhoneNumberExistsBefore.Value]));

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MinimumLength(6).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MinLength.Value]));
        }
    }
}
