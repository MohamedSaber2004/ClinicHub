using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Conversations.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderProfilePictureUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsEdited { get; set; }
        public Guid ConversationId { get; set; }

        // Media attachments
        public List<MessageMediaDto> Media { get; set; } = [];

        // Reactions
        public List<MessageReactionDto> Reactions { get; set; } = [];

        // Reply to message
        public Guid? ReplyToMessageId { get; set; }
        public ReplyToMessageDto? ReplyToMessage { get; set; }
    }

    public class MessageMediaDto
    {
        public Guid Id { get; set; }
        public MediaType MediaType { get; set; }
        public string? FileName { get; set; }
    }

    public class MessageReactionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public ReactionType ReactionType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReplyToMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

