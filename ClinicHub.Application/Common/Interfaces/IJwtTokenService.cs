using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Common.Interfaces
{
    /// <summary>
    /// Generates signed JWT access tokens and opaque refresh tokens.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a signed JWT access token for the specified user and roles.
        /// </summary>
        /// <param name="user">The user to generate the token for.</param>
        /// <param name="roles">The roles assigned to the user.</param>
        /// <returns>A JWT access token string.</returns>
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);

        /// <summary>
        /// Generates a signed JWT refresh token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate the token for.</param>
        /// <returns>A JWT refresh token string.</returns>
        string GenerateRefreshToken(ApplicationUser user);
    }
}
