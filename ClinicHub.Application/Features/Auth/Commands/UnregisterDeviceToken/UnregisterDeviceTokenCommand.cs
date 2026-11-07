using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.UnregisterDeviceToken
{
    public record UnregisterDeviceTokenCommand(string FcmToken) : IRequest<string>;
}

