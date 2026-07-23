using ClinicHub.Application.Features.Plans.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Plans.Queries.GetAllPlans
{
    public class GetAllPlansQuery : IRequest<List<PlanDto>>
    {
        public bool? IsActive { get; set; }
    }
}
