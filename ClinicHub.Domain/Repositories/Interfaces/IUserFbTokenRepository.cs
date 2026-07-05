using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IUserFbTokenRepository : IGenericRepository<UserFbToken, Guid>
    {
        Task<List<UserFbToken>> GetUserTokensAsync(Guid userId);
        Task<List<UserFbToken>> GetUserTokensByPlatformAsync(Guid userId, DevicePlatform platform);
        Task<UserFbToken?> GetByTokenAsync(string token);
    }
}
