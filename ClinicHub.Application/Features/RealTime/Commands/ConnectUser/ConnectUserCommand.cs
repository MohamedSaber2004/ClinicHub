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
        private readonly IMediator _mediator;

        public ConnectUserCommandHandler(
            IChatConnectionManager chatConnectionManager, 
            ICurrentUserService currentUserService,
            IMediator mediator)
        {
            _chatConnectionManager = chatConnectionManager;
            _currentUserService = currentUserService;
            _mediator = mediator;
        }

        public async Task<bool> Handle(ConnectUserCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            _chatConnectionManager.ConnectUser(userId, request.ConnectionId);

            // Bulk deliver messages received while offline
            await _mediator.Send(new DeliverPendingMessages.DeliverPendingMessagesCommand(), cancellationToken);

            return true;
        }
    }
}
