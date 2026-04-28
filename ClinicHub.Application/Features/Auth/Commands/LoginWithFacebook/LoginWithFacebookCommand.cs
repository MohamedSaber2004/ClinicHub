using ClinicHub.Application.Features.Auth.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWithFacebook
{
    public sealed class LoginWithFacebookCommand : IRequest<AuthResponseDto>
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
