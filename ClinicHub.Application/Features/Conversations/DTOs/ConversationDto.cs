namespace ClinicHub.Application.Features.Conversations.DTOs
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? GroupPhotoUrl { get; set; }
        public bool IsGroup { get; set; }
        public DateTime? LastMessageDate { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime CreatedAt { get; set; }

        // For 1-to-1 chats
        public Guid InitiatorId { get; set; }
        public string InitiatorName { get; set; } = string.Empty;
        public string InitiatorProfilePictureUrl { get; set; } = string.Empty;
        public Guid RecipientId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientProfilePictureUrl { get; set; } = string.Empty;

        // For group chats
        public List<ConversationParticipantDto> Participants { get; set; } = [];
        public int UnreadMessageCount { get; set; }
    }
}
