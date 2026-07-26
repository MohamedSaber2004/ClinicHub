using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.ChangePassword
{
    public class ChangeClinicUserPasswordCommandHandler : IRequestHandler<ChangeClinicUserPasswordCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public ChangeClinicUserPasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(ChangeClinicUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null || user.ClinicId != clinicId || user.IsDeleted)
                throw new NotFoundException(LocalizationKeys.AuthMessages.UserNotFound.Value);

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(nameof(UserType.Staff)) && !roles.Contains(nameof(UserType.Doctor)))
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!addResult.Succeeded)
                throw new BadRequestException(LocalizationKeys.AuthMessages.WeakPassword.Value);

            return true;
        }
    }
}
