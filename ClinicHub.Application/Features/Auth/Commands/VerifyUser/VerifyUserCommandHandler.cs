using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Auth.Commands.VerifyUser
{
    public class VerifyUserCommandHandler : IRequestHandler<VerifyUserCommand, string>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyUserCommandHandler(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (!user!.EmailConfirmed)
            {
                user.EmailConfirmed = true;
            }

            user.ClearVerificationCode();

            var result =await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.VerificationSuccess.Value);
            }

            return JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AuthMessages.InvalidVerificationCode.Value);
        }
    }
}
