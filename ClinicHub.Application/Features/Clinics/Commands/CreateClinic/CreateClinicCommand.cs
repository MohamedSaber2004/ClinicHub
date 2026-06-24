using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.CreateClinic
{
    public record CreateClinicCommand(CreateClinicDto Dto) : IRequest<ClinicManagementDto>;
}
