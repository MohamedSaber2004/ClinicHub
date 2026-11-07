using MediatR;

namespace ClinicHub.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public record MarkNotificationAsReadCommand(Guid Id) : IRequest<int>;
}
