using ClinicHub.Application.Features.Auth.Commands.ForgetPassword;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Auth.Commands.VerifyResetToken
{
    public sealed class VerifyResetTokenCommandHandler : IRequestHandler<VerifyResetTokenCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyResetTokenCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(VerifyResetTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return false;

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow) return false;

            var submittedHash = ForgetPasswordCommandHandler.ComputeSha256Hash(request.Token);
            return string.Equals(user.PasswordResetToken, submittedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
