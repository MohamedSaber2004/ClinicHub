using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.SendTestPushNotification
{
    public record SendTestPushCommand(Guid UserId, string? Title = null, string? Body = null) : IRequest<TestPushResultDto>;
}
