using MediatR;

namespace ClinicHub.Application.Features.Notifications.Queries.GetNotificationsCount
{
    public class GetNotificationsCountQuery : IRequest<int>
    {
        public bool? IsRead { get; set; }
    }
}
