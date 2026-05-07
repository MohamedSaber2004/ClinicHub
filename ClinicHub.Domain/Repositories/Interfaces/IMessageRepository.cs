using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IMessageRepository : IGenericRepository<Message, Guid>
    {
        Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(Guid conversationId, Guid recipientId, CancellationToken ct = default);
    }
}
