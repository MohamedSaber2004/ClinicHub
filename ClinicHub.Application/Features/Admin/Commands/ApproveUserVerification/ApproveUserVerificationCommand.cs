using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.ApproveUserVerification
{
    public record ApproveUserVerificationCommand(Guid UserId) : IRequest<bool>;
}
