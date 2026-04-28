using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken, Guid>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
