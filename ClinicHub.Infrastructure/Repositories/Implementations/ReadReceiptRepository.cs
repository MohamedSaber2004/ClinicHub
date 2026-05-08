using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class ReadReceiptRepository : GenericRepository<ReadReceipt, Guid>, IReadReceiptRepository
    {
        private readonly ClinicHubContext _context;

        public ReadReceiptRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ReadReceipt?> GetReadReceiptAsync(Guid messageId, Guid userId)
        {
            return await _context.Set<ReadReceipt>()
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && !r.IsDeleted);
        }

        public async Task<List<ReadReceipt>> GetMessageReadReceiptsAsync(Guid messageId)
        {
            return await _context.Set<ReadReceipt>()
                .Where(r => r.MessageId == messageId && !r.IsDeleted)
                .ToListAsync();
        }
    }
}
