using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;
using NetTopologySuite.Geometries;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IClinicRepository : IGenericRepository<Clinic, Guid>
    {
        Task<IEnumerable<Clinic>> GetNearestAsync(Point userLocation, int count, Guid? specializationId, CancellationToken cancellationToken);
        Task<IEnumerable<Clinic>> GetWithinDistanceAsync(Point userLocation, double distanceInMeters, Guid? specializationId, CancellationToken cancellationToken);
    }
}
