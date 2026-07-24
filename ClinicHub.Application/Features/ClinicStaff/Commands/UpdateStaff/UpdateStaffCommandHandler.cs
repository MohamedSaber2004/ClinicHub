using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.UpdateStaff
{
    public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, bool>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateStaffCommandHandler(ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<bool> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var user = await _userManager.FindByIdAsync(request.StaffId.ToString());
            if (user == null || user.ClinicId != clinicId || user.IsDeleted)
                throw new NotFoundException(LocalizationKeys.AuthMessages.UserNotFound.Value);

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(nameof(UserType.Staff)))
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.UpdateFullName(request.FullName);

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.UpdatePhoneNumber(request.PhoneNumber);

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            await _userManager.UpdateAsync(user);
            return true;
        }
    }
}
