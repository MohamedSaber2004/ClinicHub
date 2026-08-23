using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Application.Features.Admin.Queries.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetAppointmentsSummary
{
    public class GetAppointmentsSummaryQueryHandler : IRequestHandler<GetAppointmentsSummaryQuery, List<AppointmentsSummaryPointDto>>
    {
        private static readonly AppointmentStatus[] CancelledLike =
        {
            AppointmentStatus.Cancelled,
            AppointmentStatus.Rejected,
            AppointmentStatus.NoShow
        };

        private static readonly AppointmentStatus[] PendingLike =
        {
            AppointmentStatus.Pending,
            AppointmentStatus.Confirmed,
            AppointmentStatus.Accepted,
            AppointmentStatus.Reserved
        };

        private readonly IUnitOfWork _unitOfWork;

        public GetAppointmentsSummaryQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AppointmentsSummaryPointDto>> Handle(GetAppointmentsSummaryQuery request, CancellationToken cancellationToken)
        {
            var granularity = GraphPeriodHelper.ParseGranularity(request.Granularity);
            var (fromDate, toDate) = GraphPeriodHelper.NormalizeRange(request.FromDate, request.ToDate);

            var rows = await _unitOfWork.GetRepository<Appointment, Guid>()
                .GetAllAsync(a => !a.IsDeleted
                    && a.AppointmentDate >= fromDate
                    && a.AppointmentDate < toDate)
                .Select(a => new { a.AppointmentDate, a.Status })
                .ToListAsync(cancellationToken);

            var grouped = rows
                .GroupBy(r => GraphPeriodHelper.BucketStart(r.AppointmentDate, granularity))
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Completed = g.Count(x => x.Status == AppointmentStatus.Completed),
                        Cancelled = g.Count(x => CancelledLike.Contains(x.Status)),
                        Pending = g.Count(x => PendingLike.Contains(x.Status))
                    });

            return GraphPeriodHelper.BuildBuckets(fromDate, toDate, granularity)
                .Select(b =>
                {
                    grouped.TryGetValue(b, out var v);
                    return new AppointmentsSummaryPointDto
                    {
                        Period = GraphPeriodHelper.FormatBucket(b, granularity),
                        CompletedCount = v.Completed,
                        CancelledCount = v.Cancelled,
                        PendingCount = v.Pending
                    };
                })
                .ToList();
        }
    }
}
