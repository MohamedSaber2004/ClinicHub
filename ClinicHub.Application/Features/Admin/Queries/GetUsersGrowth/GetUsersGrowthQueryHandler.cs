using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Application.Features.Admin.Queries.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetUsersGrowth
{
    public class GetUsersGrowthQueryHandler : IRequestHandler<GetUsersGrowthQuery, List<UsersGrowthPointDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUsersGrowthQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<UsersGrowthPointDto>> Handle(GetUsersGrowthQuery request, CancellationToken cancellationToken)
        {
            var granularity = GraphPeriodHelper.ParseGranularity(request.Granularity);
            var (fromDate, toDate) = GraphPeriodHelper.NormalizeRange(request.FromDate, request.ToDate);

            var totalBeforeRange = await _userManager.Users
                .CountAsync(u => !u.IsDeleted && u.CreatedAt < fromDate, cancellationToken);

            var rows = await _userManager.Users
                .Where(u => !u.IsDeleted && u.CreatedAt >= fromDate && u.CreatedAt < toDate)
                .Select(u => new { u.CreatedAt })
                .ToListAsync(cancellationToken);

            var grouped = rows
                .GroupBy(r => GraphPeriodHelper.BucketStart(r.CreatedAt, granularity))
                .ToDictionary(g => g.Key, g => g.Count());

            var runningTotal = totalBeforeRange;

            return GraphPeriodHelper.BuildBuckets(fromDate, toDate, granularity)
                .Select(b =>
                {
                    grouped.TryGetValue(b, out var newUsers);
                    runningTotal += newUsers;
                    return new UsersGrowthPointDto
                    {
                        Period = GraphPeriodHelper.FormatBucket(b, granularity),
                        NewUsers = newUsers,
                        TotalUsers = runningTotal
                    };
                })
                .ToList();
        }
    }
}
