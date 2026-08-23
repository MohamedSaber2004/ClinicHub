using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetSubscriptionsByPlan
{
    public record GetSubscriptionsByPlanQuery(DateTime? FromDate = null, DateTime? ToDate = null)
        : IRequest<List<SubscriptionsByPlanDto>>;
}
