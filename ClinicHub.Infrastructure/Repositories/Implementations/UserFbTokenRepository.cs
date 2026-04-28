using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class UserFbTokenRepository : GenericRepository<UserFbToken, Guid>, IUserFbTokenRepository
    {
        public UserFbTokenRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
