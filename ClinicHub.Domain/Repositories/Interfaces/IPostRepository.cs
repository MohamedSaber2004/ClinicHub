using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post, Guid>
    {
        Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    }
}
