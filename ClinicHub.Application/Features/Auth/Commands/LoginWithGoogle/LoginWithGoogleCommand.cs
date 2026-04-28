using ClinicHub.Application.Features.Auth.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWithGoogle
{
    public sealed record LoginWithGoogleCommand(string IdToken) : IRequest<AuthResponseDto>;
}
