using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.RejectClinic
{
    public record RejectClinicCommand(Guid ClinicId, string? Reason = null) : IRequest<bool>;
}
