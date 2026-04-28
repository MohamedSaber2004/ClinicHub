using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ClinicHub.Infrastructure.Services
{
    public class GoogleAuth : IGoogleAuth
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly GoogleAuthSettings _googleAuthSettings;

        public GoogleAuth(SignInManager<ApplicationUser> signInManager, IOptions<GoogleAuthSettings> googleAuthSettings)
        {
            _signInManager = signInManager;
            _googleAuthSettings = googleAuthSettings.Value;
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken, CancellationToken cancellationToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleAuthSettings.WebClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch (InvalidJwtException)
            {
                return null!;
            }
        }

        public async Task LinkGoogleAccountIfNeeded(ApplicationUser user, GoogleJsonWebSignature.Payload payload, string correlationId)
        {
            var logins = await _signInManager.UserManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == "Google"))
            {
                var addLoginResult = await _signInManager.UserManager.AddLoginAsync(
                    user,
                    new UserLoginInfo("Google", payload.Subject, "Google")
                );
            }
        }

        public async Task UpdateUserInfoFromGoogle(ApplicationUser user, GoogleJsonWebSignature.Payload payload, string correlationId)
        {
            bool needsUpdate = false;
            if (user.FullName != payload.Name && !string.IsNullOrEmpty(payload.Name))
            {
                user.UpdateFullName(payload.Name);
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                var updateResult = await _signInManager.UserManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new Exception($"Failed to update user info from Google for user {user.Id}. CorrelationId: {correlationId}");
                }
            }
        }
    }
}
