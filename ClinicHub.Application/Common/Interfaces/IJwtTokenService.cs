using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);

        string GenerateRefreshToken(ApplicationUser user);
    }
}
