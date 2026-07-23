using MediatR;

namespace ClinicHub.Application.Features.Admin.Commands.ApproveClinic
{
    public record ApproveClinicCommand(Guid ClinicId) : IRequest<bool>;
}
