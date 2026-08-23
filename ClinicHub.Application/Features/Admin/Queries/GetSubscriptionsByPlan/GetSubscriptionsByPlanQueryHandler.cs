using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Application.Features.Admin.Queries.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetSubscriptionsByPlan
{
    public class GetSubscriptionsByPlanQueryHandler : IRequestHandler<GetSubscriptionsByPlanQuery, List<SubscriptionsByPlanDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSubscriptionsByPlanQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SubscriptionsByPlanDto>> Handle(GetSubscriptionsByPlanQuery request, CancellationToken cancellationToken)
        {
            var (fromDate, toDate) = GraphPeriodHelper.NormalizeRange(request.FromDate, request.ToDate);

            var rows = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.Status == SubscriptionStatus.Active
                    && s.StartDate < toDate
                    && s.EndDate >= fromDate)
                .Select(s => new
                {
                    PlanId = s.PlanId ?? Guid.Empty,
                    PlanName = s.Plan != null ? (s.Plan.NameAr ?? s.Plan.Name) : "بدون باقة",
                    s.Amount
                })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(r => r.PlanId)
                .Select(g =>
                {
                    var first = g.First();
                    return new SubscriptionsByPlanDto
                    {
                        PlanId = first.PlanId,
                        PlanName = first.PlanName,
                        SubscriptionsCount = g.Count(),
                        TotalRevenue = g.Sum(x => x.Amount)
                    };
                })
                .OrderByDescending(p => p.SubscriptionsCount)
                .ToList();
        }
    }
}
