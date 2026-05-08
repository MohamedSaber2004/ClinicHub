using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Message : BaseEntity<Guid>
    {
        public Guid ConversationId { get; private set; }
        public Guid SenderId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public MessageStatus Status { get; private set; } = MessageStatus.Pending;
        public Guid? ReplyToMessageId { get; private set; }
        public bool IsEdited { get; private set; }
        public DateTime? EditedAt { get; private set; }

        private readonly List<MessageReaction> _reactions = [];
        private readonly List<MessageMedia> _media = [];

        public IReadOnlyCollection<MessageReaction> Reactions => [.. _reactions];
        public IReadOnlyCollection<MessageMedia> Media => [.. _media];

        public Message? ReplyToMessage { get; private set; }

        private Message()
        {
        }

        public Message(Guid conversationId, Guid senderId, string content, Guid? replyToMessageId = null)
        {
            ConversationId = conversationId;
            SenderId = senderId;
            Content = content;
            ReplyToMessageId = replyToMessageId;
            IsRead = false;
            Status = MessageStatus.Pending;
            IsEdited = false;
        }

        public void MarkAsRead(Guid readByUserId)
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
                Status = MessageStatus.Read;
            }
        }

        public void MarkAsDelivered()
        {
            if (Status == MessageStatus.Pending)
            {
                Status = MessageStatus.Delivered;
            }
        }

        public void MarkAsSent()
        {
            if (Status == MessageStatus.Pending)
            {
                Status = MessageStatus.Sent;
            }
        }

        public void MarkAsFailed()
        {
            Status = MessageStatus.Failed;
        }

        public void UpdateContent(string newContent)
        {
            Content = newContent;
            IsEdited = true;
            EditedAt = DateTime.UtcNow;
        }

        public void AddReaction(MessageReaction reaction)
        {
            // Remove existing reaction from same user if any
            var existingReaction = _reactions.FirstOrDefault(r => r.UserId == reaction.UserId);
            if (existingReaction != null)
            {
                _reactions.Remove(existingReaction);
            }
            _reactions.Add(reaction);
        }

        public void RemoveReaction(Guid userId)
        {
            var reaction = _reactions.FirstOrDefault(r => r.UserId == userId);
            if (reaction != null)
            {
                _reactions.Remove(reaction);
            }
        }

        public void AddMedia(MessageMedia media)
        {
            _media.Add(media);
        }

        public void RemoveMedia(Guid mediaId)
        {
            var media = _media.FirstOrDefault(m => m.Id == mediaId);
            if (media != null)
            {
                _media.Remove(media);
            }
        }
    }
}
