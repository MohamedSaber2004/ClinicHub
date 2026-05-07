using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IConversationRepository : IGenericRepository<Conversation, Guid>
    {
        Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken ct = default);
        Task<Conversation?> GetConversationBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken ct = default);
        Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}
