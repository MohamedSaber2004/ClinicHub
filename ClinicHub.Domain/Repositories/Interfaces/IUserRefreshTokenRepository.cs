using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IUserRefreshTokenRepository : IGenericRepository<UserRefreshToken, Guid>
    {
    }
}
