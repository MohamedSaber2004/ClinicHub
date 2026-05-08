using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.ConnectUser
{
    public class ConnectUserCommand : IRequest<bool>
    {
        public string ConnectionId { get; set; } = string.Empty;
    }

    public class ConnectUserCommandHandler : IRequestHandler<ConnectUserCommand, bool>
    {
        private readonly IChatConnectionManager _chatConnectionManager;
        private readonly ICurrentUserService _currentUserService;

        public ConnectUserCommandHandler(IChatConnectionManager chatConnectionManager, ICurrentUserService currentUserService)
        {
            _chatConnectionManager = chatConnectionManager;
            _currentUserService = currentUserService;
        }

        public Task<bool> Handle(ConnectUserCommand request, CancellationToken cancellationToken)
        {
            _chatConnectionManager.ConnectUser(_currentUserService.UserId, request.ConnectionId);
            return Task.FromResult(true);
        }
    }
}
