using MediatR;

namespace ClinicHub.Application.Features.Conversations.Commands.UpdateConversationParticipantSettings
{
    public record UpdateConversationParticipantSettingsCommand(
        Guid ConversationId,
        bool? IsMuted = null,
        bool? IsArchived = null,
        bool? IsBlocked = null) : IRequest<string>;
}
