using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.HandlePusherWebhook
{
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

                    Guid? targetUserId = null;

                    // Parse UserId if provided directly
                    if (!string.IsNullOrEmpty(evt.UserId) && Guid.TryParse(evt.UserId, out var parsedUserId))
                    {
                        targetUserId = parsedUserId;
                    }
                    // Extract UserId from channel name if not provided (e.g., "private-user-{userId}")
                    else if (!string.IsNullOrEmpty(evt.Channel) && evt.Channel.StartsWith("private-user-"))
                    {
                        var userIdStr = evt.Channel.Substring("private-user-".Length);
                        if (Guid.TryParse(userIdStr, out var extractedUserId))
                        {
                            targetUserId = extractedUserId;
                        }
                    }

                    if (targetUserId.HasValue)
                    {
                        // Clean up active conversation
                        _chatConnectionManager.SetActiveConversation(targetUserId.Value, null);

                        // Also disconnect any lingering connections for this user
                        var connections = _chatConnectionManager.GetUserConnections(targetUserId.Value).ToList();
                        foreach (var connection in connections)
                        {
                            _chatConnectionManager.DisconnectUser(targetUserId.Value, connection);
                        }
                    }
                }
            }

            return Task.FromResult(true);
        }
    }
}
