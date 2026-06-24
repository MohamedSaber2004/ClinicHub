using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.ActivateClinic
{
    public record ActivateClinicCommand(Guid Id) : IRequest<ClinicManagementDto>;
}
