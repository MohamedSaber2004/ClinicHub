using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Notifications.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Notifications.Queries.GetAllNotificationsPagginated
{
    public class GetAllNotificationsPagginatedQuery: IRequest<PagginatedResult<NotificationDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
