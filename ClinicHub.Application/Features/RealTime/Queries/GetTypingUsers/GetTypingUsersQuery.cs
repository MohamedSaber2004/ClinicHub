using MediatR;

namespace ClinicHub.Application.Features.RealTime.Queries.GetTypingUsers
{
    public class GetTypingUsersQuery : IRequest<List<Guid>>
    {
        public Guid ConversationId { get; set; }
    }
}
