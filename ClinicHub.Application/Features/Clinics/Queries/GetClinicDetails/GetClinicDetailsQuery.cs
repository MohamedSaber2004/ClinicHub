using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicDetails
{
    public record GetClinicDetailsQuery(Guid Id) : IRequest<ClinicDetailsDto>;
}
