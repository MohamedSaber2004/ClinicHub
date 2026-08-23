using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Application.Features.Admin.Queries.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicsGrowth
{
    public class GetClinicsGrowthQueryHandler : IRequestHandler<GetClinicsGrowthQuery, List<ClinicsGrowthPointDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClinicsGrowthQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ClinicsGrowthPointDto>> Handle(GetClinicsGrowthQuery request, CancellationToken cancellationToken)
        {
            var granularity = GraphPeriodHelper.ParseGranularity(request.Granularity);
            var (fromDate, toDate) = GraphPeriodHelper.NormalizeRange(request.FromDate, request.ToDate);

            var clinicQuery = _unitOfWork.GetRepository<Clinic, Guid>()
                .GetAllAsync(c => !c.IsDeleted);

            var totalBeforeRange = await clinicQuery
                .CountAsync(c => c.CreatedAt < fromDate, cancellationToken);

            var rows = await clinicQuery
                .Where(c => c.CreatedAt >= fromDate && c.CreatedAt < toDate)
                .Select(c => new { c.CreatedAt })
                .ToListAsync(cancellationToken);

            var grouped = rows
                .GroupBy(r => GraphPeriodHelper.BucketStart(r.CreatedAt, granularity))
                .ToDictionary(g => g.Key, g => g.Count());

            var runningTotal = totalBeforeRange;

            return GraphPeriodHelper.BuildBuckets(fromDate, toDate, granularity)
                .Select(b =>
                {
                    grouped.TryGetValue(b, out var newClinics);
                    runningTotal += newClinics;
                    return new ClinicsGrowthPointDto
                    {
                        Period = GraphPeriodHelper.FormatBucket(b, granularity),
                        NewClinics = newClinics,
                        TotalClinics = runningTotal
                    };
                })
                .ToList();
        }
    }
}
