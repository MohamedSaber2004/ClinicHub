using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.DisconnectUser
{
    public class DisconnectUserCommand : IRequest<bool>
    {
        public string ConnectionId { get; set; } = string.Empty;
    }
}
