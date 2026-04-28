using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.ResetPassword
{
    /// <summary>
    /// Command to reset a user's password.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Token">The reset token.</param>
    /// <param name="NewPassword">The new password.</param>
    /// <param name="ConfirmPassword">The confirmation password.</param>
    public record ResetPasswordCommand(
        string Email,
        string Token,
        string NewPassword,
        string ConfirmPassword) : IRequest<Unit>;
}
