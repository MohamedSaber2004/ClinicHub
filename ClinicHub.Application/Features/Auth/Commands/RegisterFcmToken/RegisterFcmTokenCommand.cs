using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.RegisterFcmToken
{
    public record RegisterFcmTokenCommand(
        string FcmToken,
        DevicePlatform? DevicePlatform) : IRequest<string>;
}
