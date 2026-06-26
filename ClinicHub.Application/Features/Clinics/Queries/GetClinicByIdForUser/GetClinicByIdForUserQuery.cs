using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicByIdForUser
{
    public record GetClinicByIdForUserQuery(Guid Id) : IRequest<ClinicManagementDto>;
}
