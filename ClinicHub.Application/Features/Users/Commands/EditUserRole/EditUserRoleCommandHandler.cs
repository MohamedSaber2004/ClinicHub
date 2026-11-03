using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Users.Commands.EditUserRole
{
    public class EditUserRoleCommandHandler : IRequestHandler<EditUserRoleCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ICurrentUserService _currentUserService;

        public EditUserRoleCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(EditUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException(LocalizationKeys.AuthMessages.UserNotFound.Value);
            }

            var roleName = request.NewRole.ToString();
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                throw new BadRequestException(LocalizationKeys.AuthMessages.RoleAssignmentFailed.Value);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);
            }

            var addResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);
            }

            // Clinic-bound roles are unusable without a clinic (login mints no ClinicId
            // claim and every clinic-scoped endpoint 403s), so bind one here: keep the
            // user's own clinic first, otherwise inherit the acting user's clinic.
            if (request.NewRole is UserType.ClinicOwner or UserType.Doctor or UserType.Staff)
            {
                var clinicId = user.ClinicId ?? _currentUserService.CurrentClinicId;
                if (!clinicId.HasValue)
                    throw new BadRequestException(LocalizationKeys.ValidationMessages.Required.Value);

                user.AssignToClinic(clinicId.Value);
                await _userManager.UpdateAsync(user);
            }

            return true;
        }
    }
}
