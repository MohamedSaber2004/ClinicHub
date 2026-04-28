using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.ForgetPassword
{
    /// <summary>
    /// Command to initiate the password reset process.
    /// </summary>
    /// <param name="Email">The email address of the user.</param>
    public record ForgetPasswordCommand(string Email) : IRequest<Unit>;
}
