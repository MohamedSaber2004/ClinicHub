using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.SendTestPushNotification
{
    public record SendTestPushCommand(
        Guid UserId,
        NotificationType Type = NotificationType.AppointmentReminder) : IRequest<TestPushResultDto>;
}
