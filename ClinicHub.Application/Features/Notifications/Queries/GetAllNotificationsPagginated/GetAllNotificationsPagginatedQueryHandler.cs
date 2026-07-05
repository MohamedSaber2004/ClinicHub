using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Notifications.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Notifications.Queries.GetAllNotificationsPagginated
{
    public class GetAllNotificationsPagginatedQueryHandler : IRequestHandler<GetAllNotificationsPagginatedQuery, PagginatedResult<NotificationDto>>
    {
        public Task<PagginatedResult<NotificationDto>> Handle(GetAllNotificationsPagginatedQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
