namespace ClinicHub.Application.Features.Conversations.DTOs
{
    public class ConversationDetailDto
    {
        public Guid Id { get; set; }
        public Guid InitiatorId { get; set; }
        public string InitiatorName { get; set; } = string.Empty;
        public string InitiatorProfilePictureUrl { get; set; } = string.Empty;
        public Guid RecipientId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientProfilePictureUrl { get; set; } = string.Empty;
        public DateTime? LastMessageDate { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<MessageDto> Messages { get; set; } = [];
    }
}
