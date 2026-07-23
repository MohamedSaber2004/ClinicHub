using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWeb
{
    public record LoginWebCommand(
        string Email,
        string Password,
        string? FcmToken = null,
        DevicePlatform? DevicePlatform = null) : IRequest<AuthResponseDto>;
}
