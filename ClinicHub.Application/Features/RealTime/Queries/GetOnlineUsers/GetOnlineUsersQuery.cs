using MediatR;

namespace ClinicHub.Application.Features.RealTime.Queries.GetOnlineUsers
{
    public class GetOnlineUsersQuery : IRequest<IEnumerable<Guid>>
    {
    }
}
