using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Specializations.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Specializations.Queries.GetAllSpecializations
{
    public record GetAllSpecializationsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagginatedResult<SpecializationDto>>;
}
