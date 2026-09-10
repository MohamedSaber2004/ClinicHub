using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Users.Commands.AssignUserRole
{
    public class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ICurrentUserService _currentUserService;

        public AssignUserRoleCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            var roleName = request.Role.ToString();
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                throw new BadRequestException(LocalizationKeys.AuthMessages.RoleAssignmentFailed.Value);
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);
            }

            // Clinic-bound roles are unusable without a clinic (login mints no ClinicId
            // claim and every clinic-scoped endpoint 403s), so bind one here: keep the
            // user's own clinic first, otherwise inherit the acting user's clinic.
            if (request.Role is UserType.ClinicOwner or UserType.Doctor or UserType.Staff)
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
