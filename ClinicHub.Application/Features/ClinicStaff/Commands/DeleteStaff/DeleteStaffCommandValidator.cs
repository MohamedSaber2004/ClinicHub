using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.DeleteStaff
{
    public class DeleteStaffCommandValidator : AbstractValidator<DeleteStaffCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteStaffCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
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
        }
    }
}
