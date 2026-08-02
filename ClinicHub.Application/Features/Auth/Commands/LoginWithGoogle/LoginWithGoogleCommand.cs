using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWithGoogle
{
    public sealed record LoginWithGoogleCommand(
        string IdToken,
        string? FcmToken = null,
        DevicePlatform? DevicePlatform = null) : IRequest<AuthResponseDto>;
}
