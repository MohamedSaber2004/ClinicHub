using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    /// <summary>
    /// Represents a conversation between one or more users.
    /// Supports both 1-to-1 chats and group conversations.
    /// </summary>
    public class Conversation : BaseEntity<Guid>
    {
        public string? Name { get; private set; } // For group chats
        public string? GroupPhotoUrl { get; private set; }
        public Guid? CreatedByUserId { get; private set; }
        public bool IsGroup { get; private set; }
        
        // Legacy 1-to-1 fields (kept for backward compatibility)
        public Guid InitiatorId { get; private set; }
        public Guid RecipientId { get; private set; }
        
        public DateTime? LastMessageDate { get; private set; }
        public string? LastMessageContent { get; private set; }

        private readonly List<Message> _messages = [];
        private readonly List<ConversationParticipant> _participants = [];

        public IReadOnlyCollection<Message> Messages => [.. _messages];
        public IReadOnlyCollection<ConversationParticipant> Participants => [.. _participants];

        private Conversation()
        {
        }

        // Constructor for 1-to-1 conversation (backward compatible)
        public Conversation(Guid initiatorId, Guid recipientId)
        {
            InitiatorId = initiatorId;
            RecipientId = recipientId;
            IsGroup = false;
            CreatedByUserId = initiatorId;
        }

        // Constructor for group conversation
        public Conversation(string name, Guid createdByUserId, string? groupPhotoUrl = null)
        {
            Name = name;
            CreatedByUserId = createdByUserId;
            GroupPhotoUrl = groupPhotoUrl;
            IsGroup = true;
        }

        public void UpdateGroupInfo(string name, string? photoUrl)
        {
            Name = name;
            GroupPhotoUrl = photoUrl;
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
            LastMessageDate = DateTime.UtcNow;
            LastMessageContent = message.Content;
        }

        public void RemoveMessage(Message message)
        {
            _messages.Remove(message);
        }

        public void AddParticipant(ConversationParticipant participant)
        {
            if (!_participants.Any(p => p.UserId == participant.UserId))
            {
                _participants.Add(participant);
            }
        }

        public void RemoveParticipant(Guid userId)
        {
            var participant = _participants.FirstOrDefault(p => p.UserId == userId);
            if (participant != null)
            {
                _participants.Remove(participant);
            }
        }

        public ConversationParticipant? GetParticipant(Guid userId)
        {
            return _participants.FirstOrDefault(p => p.UserId == userId && !p.IsDeleted);
        }
    }
}
