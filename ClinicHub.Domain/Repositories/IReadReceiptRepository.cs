using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories
{
    public interface IReadReceiptRepository : IGenericRepository<ReadReceipt, Guid>
    {
        Task<ReadReceipt?> GetReadReceiptAsync(Guid messageId, Guid userId);
        Task<List<ReadReceipt>> GetMessageReadReceiptsAsync(Guid messageId);
    }
}
