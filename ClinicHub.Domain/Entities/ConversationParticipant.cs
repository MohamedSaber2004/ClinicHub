using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    /// <summary>
    /// Represents a participant in a conversation.
    /// Supports both 1-to-1 and group conversations.
    /// </summary>
    public class ConversationParticipant : BaseEntity<Guid>
    {
        public Guid ConversationId { get; private set; }
        public Guid UserId { get; private set; }
        public bool IsMuted { get; private set; }
        public bool IsArchived { get; private set; }
        public bool IsBlocked { get; private set; }
        public DateTime JoinedAt { get; private set; }

        public Conversation Conversation { get; private set; } = null!;

        private ConversationParticipant()
        {
        }

        public ConversationParticipant(Guid conversationId, Guid userId)
        {
            ConversationId = conversationId;
            UserId = userId;
            IsMuted = false;
            IsArchived = false;
            IsBlocked = false;
            JoinedAt = DateTime.UtcNow;
        }

        public void ToggleMute(bool muted) => IsMuted = muted;
        public void ToggleArchive(bool archived) => IsArchived = archived;
        public void ToggleBlock(bool blocked) => IsBlocked = blocked;
    }
}
