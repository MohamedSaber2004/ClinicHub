using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Queries.GetTypingUsers
{
    public class GetTypingUsersQueryHandler : IRequestHandler<GetTypingUsersQuery, List<Guid>>
    {
        private readonly IChatConnectionManager _chatConnectionManager;

        public GetTypingUsersQueryHandler(IChatConnectionManager chatConnectionManager)
        {
            _chatConnectionManager = chatConnectionManager;
        }

        public Task<List<Guid>> Handle(GetTypingUsersQuery request, CancellationToken cancellationToken)
        {
            var typingUsers = _chatConnectionManager.GetTypingUsers(request.ConversationId).ToList();
            return Task.FromResult(typingUsers);
        }
    }
}
