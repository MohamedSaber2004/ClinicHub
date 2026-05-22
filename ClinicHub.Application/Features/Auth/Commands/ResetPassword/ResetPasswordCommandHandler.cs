using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Messages> _localizer;

        public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            var removeResult = await _userManager.RemovePasswordAsync(user!);
            if (!removeResult.Succeeded)
                throw new BadRequestException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ExceptionMessages.BadRequest.Value]));

            var addResult = await _userManager.AddPasswordAsync(user!, request.NewPassword);
            if (!addResult.Succeeded)
                throw new BadRequestException(JsonLocalizationProvider.GetLocalizedString(_localizer[LocalizationKeys.ExceptionMessages.BadRequest.Value]));
            user!.ClearPasswordResetToken();
            await _userManager.UpdateAsync(user);

            return Unit.Value;
        }
    }
}
