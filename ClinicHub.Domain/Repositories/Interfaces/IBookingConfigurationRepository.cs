using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IBookingConfigurationRepository : IGenericRepository<BookingConfiguration, Guid>
    {
        Task<BookingConfiguration?> GetByClinicIdAsync(Guid clinicId);
    }
}
