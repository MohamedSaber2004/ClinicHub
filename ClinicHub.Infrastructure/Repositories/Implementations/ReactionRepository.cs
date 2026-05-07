using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class ReactionRepository : GenericRepository<Reaction, Guid>, IReactionRepository
    {
        public ReactionRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
