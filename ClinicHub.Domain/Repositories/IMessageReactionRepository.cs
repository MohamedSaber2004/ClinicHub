using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories
{
    public interface IMessageReactionRepository : IGenericRepository<MessageReaction, Guid>
    {
        Task<MessageReaction?> GetReactionAsync(Guid messageId, Guid userId);
        Task<List<MessageReaction>> GetMessageReactionsAsync(Guid messageId);
        Task RemoveUserReactionAsync(Guid messageId, Guid userId);
    }
}
