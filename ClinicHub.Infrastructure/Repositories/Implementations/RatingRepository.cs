using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class RatingRepository : GenericRepository<Rating, Guid>, IRatingRepository
    {
        private readonly ClinicHubContext _context;

        public RatingRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<Rating>> GetDoctorRatingsAsync(Guid doctorId)
        {
            return _context.Set<Rating>()
                .Where(r => r.Type == RatingType.Doctor && r.DoctorId == doctorId && !r.IsDeleted)
                .Include(r => r.User)
                .ToListAsync();
        }

        public Task<List<Rating>> GetClinicRatingsAsync(Guid clinicId)
        {
            return _context.Set<Rating>()
                .Where(r => r.Type == RatingType.Clinic && r.ClinicId == clinicId && !r.IsDeleted)
                .Include(r => r.User)
                .ToListAsync();
        }

        public Task<List<Rating>> GetPlaceCleanlinessRatingsAsync(Guid clinicId)
        {
            return _context.Set<Rating>()
                .Where(r => r.Type == RatingType.PlaceCleanliness && r.ClinicId == clinicId && !r.IsDeleted)
                .Include(r => r.User)
                .ToListAsync();
        }

        public Task<List<Rating>> GetReceptionRatingsAsync(Guid clinicId)
        {
            return _context.Set<Rating>()
                .Where(r => r.Type == RatingType.Reception && r.ClinicId == clinicId && !r.IsDeleted)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<double?> GetDoctorAverageRatingAsync(Guid doctorId)
        {
            var ratings = await _context.Set<Rating>()
                .Where(r => r.Type == RatingType.Doctor && r.DoctorId == doctorId && !r.IsDeleted)
                .Select(r => r.Value)
                .ToListAsync();

            return ratings.Count > 0 ? ratings.Average() : null;
        }

        public async Task<double?> GetClinicAverageRatingAsync(Guid clinicId)
        {
            var ratings = await _context.Set<Rating>()
                .Where(r => r.Type == RatingType.Clinic && r.ClinicId == clinicId && !r.IsDeleted)
                .Select(r => r.Value)
                .ToListAsync();

            return ratings.Count > 0 ? ratings.Average() : null;
        }

        public async Task<int> GetClinicRatingsCountAsync(Guid clinicId)
        {
            return await _context.Set<Rating>()
                .CountAsync(r => r.Type == RatingType.Clinic && r.ClinicId == clinicId && !r.IsDeleted);
        }

        public async Task<Dictionary<Guid, (double? Average, int Count)>> GetClinicRatingSummariesAsync(IReadOnlyCollection<Guid> clinicIds)
        {
            if (clinicIds.Count == 0)
                return new Dictionary<Guid, (double? Average, int Count)>();

            var rows = await _context.Set<Rating>()
                .Where(r => r.Type == RatingType.Clinic && !r.IsDeleted && r.ClinicId != null && clinicIds.Contains(r.ClinicId.Value))
                .GroupBy(r => r.ClinicId!.Value)
                .Select(g => new { ClinicId = g.Key, Average = g.Average(r => (double)r.Value), Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => r.ClinicId, r => ((double?)r.Average, r.Count));
        }

        public Task<Rating?> GetUserRatingForDoctorAsync(Guid userId, Guid doctorId)
        {
            return _context.Set<Rating>()
                .FirstOrDefaultAsync(r => r.Type == RatingType.Doctor && r.UserId == userId && r.DoctorId == doctorId && !r.IsDeleted);
        }

        public Task<Rating?> GetUserRatingForClinicAsync(Guid userId, Guid clinicId, RatingType type)
        {
            return _context.Set<Rating>()
                .FirstOrDefaultAsync(r => r.Type == type && r.UserId == userId && r.ClinicId == clinicId && !r.IsDeleted);
        }
    }
}
