using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.DeactivateClinic
{
    public record DeactivateClinicCommand(Guid Id) : IRequest<ClinicManagementDto>;
}
