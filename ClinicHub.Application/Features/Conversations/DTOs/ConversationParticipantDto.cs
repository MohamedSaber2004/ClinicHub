namespace ClinicHub.Application.Features.Conversations.DTOs
{
    public class ConversationParticipantDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserProfilePictureUrl { get; set; } = string.Empty;
        public bool IsMuted { get; set; }
        public bool IsArchived { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
