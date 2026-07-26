using MediatR;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.ChangePassword
{
    public record ChangeClinicUserPasswordCommand(
        Guid UserId,
        string NewPassword,
        string ConfirmPassword) : IRequest<Unit>;
}
