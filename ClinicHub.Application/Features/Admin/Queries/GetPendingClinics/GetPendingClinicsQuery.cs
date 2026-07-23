using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetPendingClinics
{
    public record GetPendingClinicsQuery : IRequest<List<ClinicManagementDto>>;
}
