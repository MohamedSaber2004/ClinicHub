using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.VerifyResetToken
{
    public record VerifyResetTokenCommand(string Email, string Token) : IRequest<bool>;
}
