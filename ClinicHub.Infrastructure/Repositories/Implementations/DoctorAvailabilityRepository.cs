using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class DoctorAvailabilityRepository : GenericRepository<DoctorAvailability, Guid>, IDoctorAvailabilityRepository
    {
        public DoctorAvailabilityRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
