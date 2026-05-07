using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class MessageRepository : GenericRepository<Message, Guid>, IMessageRepository
    {
        private readonly ClinicHubContext _context;

        public MessageRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(Guid conversationId, CancellationToken ct = default)
        {
            return await _context.Set<Message>()
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(Guid conversationId, Guid recipientId, CancellationToken ct = default)
        {
            return await _context.Set<Message>()
                .Where(m => m.ConversationId == conversationId && m.SenderId != recipientId && !m.IsRead && !m.IsDeleted)
                .ToListAsync(ct);
        }
    }
}
