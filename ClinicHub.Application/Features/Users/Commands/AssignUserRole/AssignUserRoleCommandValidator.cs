using ClinicHub.Application.Common.Interfaces;
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

        public AssignUserRoleCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
        {
            _userManager = userManager;

            RuleFor(x => x.UserId).NotEmpty()
                .MustAsync((userId, cancellationToken) => UserExists(userId, cancellationToken))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]));

            RuleFor(x => x.Role).IsInEnum().NotEqual(UserType.None);

            // Clinic-bound roles need a clinic from somewhere: the user's own, or the
            // acting user's (e.g. a clinic owner granting Staff inherits their clinic).
            When(x => x.Role is UserType.ClinicOwner or UserType.Doctor or UserType.Staff, () =>
            {
                RuleFor(x => x)
                    .MustAsync(async (command, ct) =>
                    {
                        var user = await userManager.FindByIdAsync(command.UserId.ToString());
                        return user?.ClinicId.HasValue == true || currentUserService.CurrentClinicId.HasValue;
                    })
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
            });
        }

        private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
        {
            return await _userManager.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
        }
    }
}
