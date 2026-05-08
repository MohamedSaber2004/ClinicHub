using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.SetTyping
{
    public class SetTypingCommand : IRequest<bool>
    {
        public Guid ConversationId { get; set; }
        public bool IsTyping { get; set; }
    }

    public class SetTypingCommandHandler : IRequestHandler<SetTypingCommand, bool>
    {
        private readonly IChatConnectionManager _chatConnectionManager;
        private readonly IPusherService _pusherService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SetTypingCommandHandler(
            IChatConnectionManager chatConnectionManager, 
            IPusherService pusherService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _chatConnectionManager = chatConnectionManager;
            _pusherService = pusherService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(SetTypingCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            _chatConnectionManager.SetUserTyping(request.ConversationId, userId, request.IsTyping);

            var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(request.ConversationId);
            if (conversation == null) return false;

            var recipientId = conversation.InitiatorId == userId ? conversation.RecipientId : conversation.InitiatorId;

            var typingEventData = new
            {
                conversationId = request.ConversationId,
                userId = userId,
                isTyping = request.IsTyping
            };

            await _pusherService.TriggerEventAsync($"private-user-{recipientId}", "typing", typingEventData);

            return true;
        }
    }
}
