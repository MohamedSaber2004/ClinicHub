using ClinicHub.Application.Features.Specializations.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Specializations.Queries.GetSpecializationById
{
    public record GetSpecializationByIdQuery(Guid Id) : IRequest<SpecializationDto?>;
}
