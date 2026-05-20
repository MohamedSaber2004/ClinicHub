using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.ConnectUser
{
    public class ConnectUserCommand : IRequest<bool>
    {
        public string ConnectionId { get; set; } = string.Empty;
    }
}
