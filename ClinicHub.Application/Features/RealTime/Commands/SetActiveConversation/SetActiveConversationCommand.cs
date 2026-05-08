using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.SetActiveConversation
{
    public class SetActiveConversationCommand : IRequest<bool>
    {
        public Guid? ConversationId { get; set; }
    }

    public class SetActiveConversationCommandHandler : IRequestHandler<SetActiveConversationCommand, bool>
    {
        private readonly IChatConnectionManager _chatConnectionManager;
        private readonly ICurrentUserService _currentUserService;

        public SetActiveConversationCommandHandler(
            IChatConnectionManager chatConnectionManager,
            ICurrentUserService currentUserService)
        {
            _chatConnectionManager = chatConnectionManager;
            _currentUserService = currentUserService;
        }

        public Task<bool> Handle(SetActiveConversationCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            _chatConnectionManager.SetActiveConversation(userId, request.ConversationId);

            return Task.FromResult(true);
        }
    }
}
