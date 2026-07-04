using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IRatingRepository : IGenericRepository<Rating, Guid>
    {
        Task<List<Rating>> GetDoctorRatingsAsync(Guid doctorId);
        Task<List<Rating>> GetClinicRatingsAsync(Guid clinicId);
        Task<double?> GetDoctorAverageRatingAsync(Guid doctorId);
        Task<double?> GetClinicAverageRatingAsync(Guid clinicId);
        Task<Rating?> GetUserRatingForDoctorAsync(Guid userId, Guid doctorId);
        Task<Rating?> GetUserRatingForClinicAsync(Guid userId, Guid clinicId);
    }
}
