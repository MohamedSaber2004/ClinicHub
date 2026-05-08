using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Queries.GetOnlineUsers
{
    public class GetOnlineUsersQuery : IRequest<IEnumerable<Guid>>
    {
    }

    public class GetOnlineUsersQueryHandler : IRequestHandler<GetOnlineUsersQuery, IEnumerable<Guid>>
    {
        private readonly IChatConnectionManager _chatConnectionManager;

        public GetOnlineUsersQueryHandler(IChatConnectionManager chatConnectionManager)
        {
            _chatConnectionManager = chatConnectionManager;
        }

        public Task<IEnumerable<Guid>> Handle(GetOnlineUsersQuery request, CancellationToken cancellationToken)
        {
            var onlineUsers = _chatConnectionManager.GetOnlineUsers();
            return Task.FromResult(onlineUsers);
        }
    }
}
