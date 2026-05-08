using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class ConversationParticipantRepository : GenericRepository<ConversationParticipant, Guid>, IConversationParticipantRepository
    {
        private readonly ClinicHubContext _context;

        public ConversationParticipantRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ConversationParticipant>> GetConversationParticipantsAsync(Guid conversationId)
        {
            return await _context.Set<ConversationParticipant>()
                .Where(p => p.ConversationId == conversationId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<ConversationParticipant?> GetParticipantAsync(Guid conversationId, Guid userId)
        {
            return await _context.Set<ConversationParticipant>()
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted);
        }

        public async Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId)
        {
            return await _context.Set<ConversationParticipant>()
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId && !p.IsDeleted);
        }
    }
}
