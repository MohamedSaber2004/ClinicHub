using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class MessageReaction : BaseEntity<Guid>
    {
        public Guid MessageId { get; private set; }
        public Guid UserId { get; private set; }
        public ReactionType ReactionType { get; private set; }

        public Message Message { get; private set; } = null!;

        private MessageReaction()
        {
        }

        public MessageReaction(Guid messageId, Guid userId, ReactionType reactionType)
        {
            MessageId = messageId;
            UserId = userId;
            ReactionType = reactionType;
        }

        public void UpdateReaction(ReactionType reactionType)
        {
            ReactionType = reactionType;
        }
    }
}
