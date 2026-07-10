using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.RejectUserVerification
{
    public record RejectUserVerificationCommand(Guid UserId, string? Notes) : IRequest<bool>;
}
