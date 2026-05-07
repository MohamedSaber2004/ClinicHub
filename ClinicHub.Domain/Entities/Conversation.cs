using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class Conversation : BaseEntity<Guid>
    {
        public Guid InitiatorId { get; private set; }
        public Guid RecipientId { get; private set; }
        public DateTime? LastMessageDate { get; private set; }

        private readonly List<Message> _messages = [];

        public IReadOnlyCollection<Message> Messages => [.. _messages];

        private Conversation()
        {
        }

        public Conversation(Guid initiatorId, Guid recipientId)
        {
            InitiatorId = initiatorId;
            RecipientId = recipientId;
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
            LastMessageDate = DateTime.UtcNow;
        }

        public void RemoveMessage(Message message)
        {
            _messages.Remove(message);
        }
    }
}
