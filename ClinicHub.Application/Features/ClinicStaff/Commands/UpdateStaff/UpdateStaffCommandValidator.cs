using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.UpdateStaff
{
    public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateStaffCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(v => v.StaffId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, ct) =>
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null || user.IsDeleted) return false;
                    var roles = await _userManager.GetRolesAsync(user);
                    return roles.Contains(nameof(UserType.Staff));
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.StaffMessages.NotFound.Value]));

            When(v => !string.IsNullOrWhiteSpace(v.FullName), () =>
            {
                RuleFor(v => v.FullName)
                    .MaximumLength(200).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
            });

            When(v => !string.IsNullOrWhiteSpace(v.PhoneNumber), () =>
            {
                RuleFor(v => v.PhoneNumber)
                    .MaximumLength(20).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
            });

            RuleFor(v => v.Image)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(v => !string.IsNullOrWhiteSpace(v.Image))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));
        }
    }
}
