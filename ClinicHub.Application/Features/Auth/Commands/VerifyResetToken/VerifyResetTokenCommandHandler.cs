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

            if (user.PasswordResetTokenExpiry < DateTime.Now) return false;

            return string.Equals(user.PasswordResetToken, request.Token, StringComparison.Ordinal);
        }
    }
}
