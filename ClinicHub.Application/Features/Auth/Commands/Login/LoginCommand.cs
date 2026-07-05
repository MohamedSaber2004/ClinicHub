using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password,
        string? FcmToken = null,
        DevicePlatform? DevicePlatform = null) : IRequest<AuthResponseDto>;
}
