using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ClinicHub.Application.Features.Auth.Commands.ForgetPassword
{
    /// <summary>
    /// Handler for the ForgetPasswordCommand.
    /// Generates a 6-digit OTP, stores its SHA-256 hash, and emails the plain code to the user.
    /// </summary>
    public sealed class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _settings;

        public ForgetPasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<EmailSettings> settings)
        {
            _userManager = userManager;
            _emailService = emailService;
            _settings = settings.Value;
        }

        public async Task<Unit> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            var otpHash = ComputeSha256Hash(otp);
            user!.SetPasswordResetToken(otpHash, DateTime.UtcNow.AddMinutes(_settings.ForgetPasswordExpiryMinutes));
            await _userManager.UpdateAsync(user);

            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FullName, otp, cancellationToken);

            return Unit.Value;
        }

        internal static string ComputeSha256Hash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
