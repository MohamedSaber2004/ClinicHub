using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.Logout
{
    public record LogoutCommand(string RefreshToken) : IRequest<string>;
}
