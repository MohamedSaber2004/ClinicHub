using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.DisconnectUser
{
    public class DisconnectUserCommand : IRequest<bool>
    {
        public string ConnectionId { get; set; } = string.Empty;
    }

    public class DisconnectUserCommandHandler : IRequestHandler<DisconnectUserCommand, bool>
    {
        private readonly IChatConnectionManager _chatConnectionManager;
        private readonly ICurrentUserService _currentUserService;

        public DisconnectUserCommandHandler(IChatConnectionManager chatConnectionManager, ICurrentUserService currentUserService)
        {
            _chatConnectionManager = chatConnectionManager;
            _currentUserService = currentUserService;
        }

        public Task<bool> Handle(DisconnectUserCommand request, CancellationToken cancellationToken)
        {
            _chatConnectionManager.DisconnectUser(_currentUserService.UserId, request.ConnectionId);
            return Task.FromResult(true);
        }
    }
}
