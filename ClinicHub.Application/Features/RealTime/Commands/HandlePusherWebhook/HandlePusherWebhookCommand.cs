using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.HandlePusherWebhook
{
    public class HandlePusherWebhookCommand : IRequest<bool>
    {
        // Pusher webhooks contain a list of events
        public List<PusherWebhookEvent> Events { get; set; } = new();
    }

    public class PusherWebhookEvent
    {
        public string Name { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string SocketId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class HandlePusherWebhookCommandHandler : IRequestHandler<HandlePusherWebhookCommand, bool>
    {
        private readonly IChatConnectionManager _chatConnectionManager;

        public HandlePusherWebhookCommandHandler(IChatConnectionManager chatConnectionManager)
        {
            _chatConnectionManager = chatConnectionManager;
        }

        public Task<bool> Handle(HandlePusherWebhookCommand request, CancellationToken cancellationToken)
        {
            foreach (var evt in request.Events)
            {
                // When a user disconnects entirely
                if (evt.Name == "channel_vacated" || evt.Name == "member_removed")
                {
                    // If we have the specific SocketId from Pusher's payload
                    if (!string.IsNullOrEmpty(evt.SocketId))
                    {
                        _chatConnectionManager.DisconnectUser(evt.SocketId);
                    }
                    
                    // Or if we get the UserId
                    if (!string.IsNullOrEmpty(evt.UserId) && Guid.TryParse(evt.UserId, out var userId))
                    {
                        // Clean up active conversation
                        _chatConnectionManager.SetActiveConversation(userId, null);
                    }
                }
            }

            return Task.FromResult(true);
        }
    }
}
