using ClinicHub.Application.Features.Specializations.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Specializations.Queries.GetActiveSpecializations
{
    public record GetActiveSpecializationsQuery(bool IsFamous = true) : IRequest<List<SpecializationLookupDto>>;
}
