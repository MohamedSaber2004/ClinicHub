using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class ClinicRepository : GenericRepository<Clinic, Guid>, IClinicRepository
    {
        private readonly ClinicHubContext _context;

        public ClinicRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Clinic>> GetNearestAsync(Point userLocation, int count, Guid? specializationId, CancellationToken cancellationToken)
        {
            var query = _context.Clinics
                .Where(c => c.IsActive && !c.IsDeleted);

            if (specializationId.HasValue)
            {
                query = query.Where(c => c.SpecializationId == specializationId.Value);
            }

            return await query
                .OrderBy(c => c.Location.Distance(userLocation))
                .Take(count)
                .Include(c => c.Specialization)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Clinic>> GetWithinDistanceAsync(Point userLocation, double distanceInMeters, Guid? specializationId, CancellationToken cancellationToken)
        {
            var query = _context.Clinics
                .Where(c => c.IsActive && !c.IsDeleted && c.Location.IsWithinDistance(userLocation, distanceInMeters));

            if (specializationId.HasValue)
            {
                query = query.Where(c => c.SpecializationId == specializationId.Value);
            }

            return await query
                .OrderBy(c => c.Location.Distance(userLocation))
                .Include(c => c.Specialization)
                .ToListAsync(cancellationToken);
        }
    }
}
