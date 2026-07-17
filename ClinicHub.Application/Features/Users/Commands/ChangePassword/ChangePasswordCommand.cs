using MediatR;

namespace ClinicHub.Application.Features.Users.Commands.ChangePassword
{
    public record ChangePasswordCommand(
        Guid? Id,
        string? OldPassword,
        string NewPassword,
        string ConfirmPassword) : IRequest<Unit>;
}
