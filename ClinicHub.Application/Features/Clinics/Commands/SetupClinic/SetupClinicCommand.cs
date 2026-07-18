using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.SetupClinic
{
    public record SetupClinicCommand(SetupClinicDto Dto) : IRequest<ClinicManagementDto>;
}
