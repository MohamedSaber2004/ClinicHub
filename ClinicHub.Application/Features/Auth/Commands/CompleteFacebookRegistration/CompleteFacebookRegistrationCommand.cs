using ClinicHub.Application.Features.Auth.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.CompleteFacebookRegistration
{
    public class CompleteFacebookRegistrationCommand : IRequest<AuthResponseDto>
    {
        public string AccessToken { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
