using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWithFacebook
{
    public sealed class LoginWithFacebookCommand : IRequest<AuthResponseDto>
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? FcmToken { get; set; }
        public DevicePlatform? DevicePlatform { get; set; }
    }
}
