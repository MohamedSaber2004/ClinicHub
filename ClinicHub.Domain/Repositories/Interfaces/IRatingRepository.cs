using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IRatingRepository : IGenericRepository<Rating, Guid>
    {
        Task<List<Rating>> GetDoctorRatingsAsync(Guid doctorId);
        Task<List<Rating>> GetClinicRatingsAsync(Guid clinicId);
        Task<List<Rating>> GetPlaceCleanlinessRatingsAsync(Guid clinicId);
        Task<List<Rating>> GetReceptionRatingsAsync(Guid clinicId);
        Task<double?> GetDoctorAverageRatingAsync(Guid doctorId);
        Task<double?> GetClinicAverageRatingAsync(Guid clinicId);
        Task<int> GetClinicRatingsCountAsync(Guid clinicId);
        Task<Rating?> GetUserRatingForDoctorAsync(Guid userId, Guid doctorId);
        Task<Rating?> GetUserRatingForClinicAsync(Guid userId, Guid clinicId, RatingType type);
    }
}
