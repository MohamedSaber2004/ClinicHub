using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class SpecializationRepository : GenericRepository<Specialization, Guid>, ISpecializationRepository
    {
        public SpecializationRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
