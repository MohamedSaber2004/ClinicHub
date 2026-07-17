using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public ChangePasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (request.Id.HasValue)
            {
                if (_currentUserService.UserTypes is null ||
                    (_currentUserService.UserTypes.Value & (int)UserType.SuperAdmin) == 0)
                    throw new UnAuthorizedException();

                var user = await _userManager.FindByIdAsync(request.Id.Value.ToString());

                if (user is null || user.IsDeleted)
                    throw new NotFoundException(_localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]);

                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                    throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.RoleAssignmentFailed.Value]);

                var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (!addResult.Succeeded)
                    throw new BadRequestException(_localizer[LocalizationKeys.AuthMessages.WeakPassword.Value]);
            }
            else
            {
                var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());

                if (user is null)
                    throw new NotFoundException(_localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]);

                var result = await _userManager.ChangePasswordAsync(user, request.OldPassword!, request.NewPassword);

                if (!result.Succeeded)
                    throw new BadRequestException(
                        JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.AuthMessages.IncorrectOldPassword.Value]));
            }

            return Unit.Value;
        }
    }
}
