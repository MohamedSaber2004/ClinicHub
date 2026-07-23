using ClinicHub.Application.Features.Plans.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Plans.Queries.GetActivePlans
{
    public record GetActivePlansQuery : IRequest<List<PlanDto>>;
}
