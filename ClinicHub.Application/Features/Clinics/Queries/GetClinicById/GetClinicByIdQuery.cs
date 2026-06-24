using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicById
{
    public record GetClinicByIdQuery(Guid Id) : IRequest<ClinicManagementDto>;
}
