using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.HandlePusherWebhook
{
    public class HandlePusherWebhookCommand : IRequest<bool>
    {
        public List<PusherWebhookEvent> Events { get; set; } = new();
    }

    public class PusherWebhookEvent
    {
        public string Name { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string SocketId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
