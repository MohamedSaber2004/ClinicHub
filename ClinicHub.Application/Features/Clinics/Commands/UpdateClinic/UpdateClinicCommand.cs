using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinic
{
    public record UpdateClinicCommand(Guid Id, UpdateClinicDto Dto) : IRequest<ClinicManagementDto>;
}
