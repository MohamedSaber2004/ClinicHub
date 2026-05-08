using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class MessageReactionRepository : GenericRepository<MessageReaction, Guid>, IMessageReactionRepository
    {
        private readonly ClinicHubContext _context;

        public MessageReactionRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<MessageReaction?> GetReactionAsync(Guid messageId, Guid userId)
        {
            return await _context.Set<MessageReaction>()
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && !r.IsDeleted);
        }

        public async Task<List<MessageReaction>> GetMessageReactionsAsync(Guid messageId)
        {
            return await _context.Set<MessageReaction>()
                .Where(r => r.MessageId == messageId && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task RemoveUserReactionAsync(Guid messageId, Guid userId)
        {
            var reaction = await GetReactionAsync(messageId, userId);
            if (reaction != null)
            {
                _context.Set<MessageReaction>().Remove(reaction);
            }
        }
    }
}
