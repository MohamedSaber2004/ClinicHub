using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class MessageMediaRepository : GenericRepository<MessageMedia, Guid>, IMessageMediaRepository
    {
        private readonly ClinicHubContext _context;

        public MessageMediaRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<MessageMedia>> GetMessageMediaAsync(Guid messageId)
        {
            return await _context.Set<MessageMedia>()
                .Where(m => m.MessageId == messageId && !m.IsDeleted)
                .ToListAsync();
        }
    }
}
