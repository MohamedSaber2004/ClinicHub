using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class ConversationRepository : GenericRepository<Conversation, Guid>, IConversationRepository
    {
        private readonly ClinicHubContext _context;

        public ConversationRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken ct = default)
        {
            return await GetFirstWithIncluding(c => c.Id == id && !c.IsDeleted, 
                c => c.Messages)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Conversation?> GetConversationBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken ct = default)
        {
            return await GetFirstWithIncluding(c => 
                (c.InitiatorId == userId1 && c.RecipientId == userId2 || 
                 c.InitiatorId == userId2 && c.RecipientId == userId1) && !c.IsDeleted,
                c => c.Messages)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await GetAllWithIncluding(c => 
                (c.InitiatorId == userId || c.RecipientId == userId) && !c.IsDeleted,
                c => c.Messages)
                .OrderByDescending(c => c.LastMessageDate)
                .ToListAsync(ct);
        }
    }
}
