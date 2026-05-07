using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class Message : BaseEntity<Guid>
    {
        public Guid ConversationId { get; private set; }
        public Guid SenderId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }

        private Message()
        {
        }

        public Message(Guid conversationId, Guid senderId, string content)
        {
            ConversationId = conversationId;
            SenderId = senderId;
            Content = content;
            IsRead = false;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
