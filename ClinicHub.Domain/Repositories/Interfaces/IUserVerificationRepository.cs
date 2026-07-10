using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IUserVerificationRepository : IGenericRepository<UserVerification, Guid>
    {
    }
}
