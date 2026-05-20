using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories
{
    public interface IConversationParticipantRepository : IGenericRepository<ConversationParticipant, Guid>
    {
        Task<List<ConversationParticipant>> GetConversationParticipantsAsync(Guid conversationId);
        Task<ConversationParticipant?> GetParticipantAsync(Guid conversationId, Guid userId);
        Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId);
    }
}
