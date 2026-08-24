using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Application.Features.Admin.Queries.Common;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentEntity = ClinicHub.Domain.Entities.Payment;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicRevenueTrend
{
    public class GetClinicRevenueTrendQueryHandler
        : IRequestHandler<GetClinicRevenueTrendQuery, List<RevenueTrendPointDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetClinicRevenueTrendQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<RevenueTrendPointDto>> Handle(GetClinicRevenueTrendQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.CurrentClinicId is null)
                return new List<RevenueTrendPointDto>();

            var clinicId = _currentUserService.CurrentClinicId.Value;
            var granularity = GraphPeriodHelper.ParseGranularity(request.Granularity);
            var (fromDate, toDate) = GraphPeriodHelper.NormalizeRange(request.FromDate, request.ToDate);

            var rows = await _unitOfWork.GetRepository<PaymentEntity, Guid>()
                .GetAllAsync(p => p.ClinicId == clinicId
                    && p.Type == PaymentType.Appointment
                    && p.Status == PaymentStatus.Paid
                    && p.PaidAt != null
                    && p.PaidAt >= fromDate
                    && p.PaidAt < toDate)
                .Select(p => new { p.PaidAt, p.Amount })
                .ToListAsync(cancellationToken);

            var grouped = rows
                .GroupBy(r => GraphPeriodHelper.BucketStart(r.PaidAt!.Value, granularity))
                .ToDictionary(
                    g => g.Key,
                    g => (Revenue: g.Sum(x => x.Amount), Count: g.Count()));

            return GraphPeriodHelper.BuildBuckets(fromDate, toDate, granularity)
                .Select(b =>
                {
                    grouped.TryGetValue(b, out var v);
                    return new RevenueTrendPointDto
                    {
                        Period = GraphPeriodHelper.FormatBucket(b, granularity),
                        Revenue = v.Revenue,
                        PaymentsCount = v.Count
                    };
                })
                .ToList();
        }
    }
}
