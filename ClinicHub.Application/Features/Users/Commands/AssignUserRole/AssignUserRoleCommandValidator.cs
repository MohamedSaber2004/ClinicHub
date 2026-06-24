using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Commands.AssignUserRole
{
    public class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignUserRoleCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.UserId).NotEmpty()
                .MustAsync((userId, cancellationToken) => UserExists(userId, cancellationToken))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]));

            RuleFor(x => x.Role).NotEmpty()
                .Must(role => Enum.TryParse<UserType>(role, ignoreCase: true, out _))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidRole.Value]));
        }

        private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
        {
            return await _userManager.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
        }
    }
}
