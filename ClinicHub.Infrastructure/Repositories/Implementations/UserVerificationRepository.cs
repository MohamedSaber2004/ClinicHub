using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class UserVerificationRepository : GenericRepository<UserVerification, Guid>, IUserVerificationRepository
    {
        public UserVerificationRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
