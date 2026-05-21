using ClinicHub.Application.Features.Conversations.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Conversations.Commands.UpdateConversation
{
    public record UpdateConversationCommand(
        Guid ConversationId,
        string? Name = null,
        string? GroupPhotoUrl = null) : IRequest<ConversationDto>;
}
