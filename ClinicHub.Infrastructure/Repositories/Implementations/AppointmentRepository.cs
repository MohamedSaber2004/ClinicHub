using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class AppointmentRepository : GenericRepository<Appointment, Guid>, IAppointmentRepository
    {
        public AppointmentRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
