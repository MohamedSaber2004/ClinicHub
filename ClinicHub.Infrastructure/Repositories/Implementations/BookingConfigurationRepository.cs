using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class BookingConfigurationRepository : GenericRepository<BookingConfiguration, Guid>, IBookingConfigurationRepository
    {
        private readonly ClinicHubContext _context;

        public BookingConfigurationRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<BookingConfiguration?> GetByClinicIdAsync(Guid clinicId)
        {
            return await _context.BookingConfigurations
                .FirstOrDefaultAsync(bc => bc.ClinicId == clinicId && !bc.IsDeleted);
        }
    }
}
