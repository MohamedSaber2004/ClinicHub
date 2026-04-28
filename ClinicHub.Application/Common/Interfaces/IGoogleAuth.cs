using ClinicHub.Domain.Entities;
using Google.Apis.Auth;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IGoogleAuth
    {
        public Task UpdateUserInfoFromGoogle(ApplicationUser user, GoogleJsonWebSignature.Payload payload, string correlationId);

        public Task LinkGoogleAccountIfNeeded(ApplicationUser user, GoogleJsonWebSignature.Payload payload, string correlationId);

        public Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken, CancellationToken cancellationToken);
    }
}
