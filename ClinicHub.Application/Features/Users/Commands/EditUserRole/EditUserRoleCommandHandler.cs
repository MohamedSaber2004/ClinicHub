using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Users.Commands.EditUserRole
{
    public class EditUserRoleCommandHandler : IRequestHandler<EditUserRoleCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public EditUserRoleCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> Handle(EditUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException(nameof(ApplicationUser), request.UserId);
            }

            var roleExists = await _roleManager.RoleExistsAsync(request.NewRole);
            if (!roleExists)
            {
                throw new BadRequestException($"Role '{request.NewRole}' does not exist.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to remove existing roles: {errors}");
            }

            var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to add new role: {errors}");
            }

            return true;
        }
    }
}
