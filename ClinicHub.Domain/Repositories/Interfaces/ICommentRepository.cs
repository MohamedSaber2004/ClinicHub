using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment, Guid>
    {
        Task<Comment?> GetByIdWithRepliesAsync(Guid id, CancellationToken ct = default);
    }
}
