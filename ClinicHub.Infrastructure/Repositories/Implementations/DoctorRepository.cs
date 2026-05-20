using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class DoctorRepository : GenericRepository<Doctor, Guid>, IDoctorRepository
    {
        public DoctorRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
