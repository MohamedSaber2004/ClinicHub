using ClinicHub.Application.Features.Auth.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(
        string Email, 
        string Password) : IRequest<AuthResponseDto>;
}
