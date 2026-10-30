using ClinicHub.Application.Common;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPaymentStats;

public class GetAdminPaymentStatsQueryHandler : IRequestHandler<GetAdminPaymentStatsQuery, AdminPaymentStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminPaymentStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminPaymentStatsDto> Handle(GetAdminPaymentStatsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.PaymentRepository.GetAllAsync(null);

        if (request.Type.HasValue)
            query = query.Where(p => p.Type == request.Type.Value);

        var hasDateRange = request.FromDate.HasValue || request.ToDate.HasValue;

        if (request.FromDate.HasValue)
            query = query.Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date >= request.FromDate.Value.Date);

        if (request.ToDate.HasValue)
            query = query.Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date <= request.ToDate.Value.Date);

        var today = DateTime.Now.Date;

        // PartiallyRefunded payments are real revenue, net of the refunded amount.
        var todayRevenue = await query
            .Where(p => (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.PartiallyRefunded)
                && (hasDateRange || (p.PaidAt.HasValue && p.PaidAt.Value >= today)))
            .SumAsync(p => (decimal?)(p.Amount - p.RefundedAmount), cancellationToken) ?? 0;

        var appointmentRows = await query
            .Where(p => p.Type == PaymentType.Appointment
                && (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.PartiallyRefunded))
            .Select(p => new { p.Amount, p.RefundedAmount, p.PlatformFeeAmount, p.ClinicNetAmount })
            .ToListAsync(cancellationToken);
        var appointmentsRevenue = appointmentRows.Sum(p => p.Amount - p.RefundedAmount);
        var percent = await GetPlatformFeePercentAsync(cancellationToken);
        var (appointmentsFees, appointmentsNet) = PaymentRevenueResolver.SumResolved(
            appointmentRows.Select(p => (p.Amount, p.RefundedAmount, p.PlatformFeeAmount, p.ClinicNetAmount)), percent);

        var subscriptionsRevenue = await query
            .Where(p => p.Type == PaymentType.Subscription
                && (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.PartiallyRefunded))
            .SumAsync(p => (decimal?)(p.Amount - p.RefundedAmount), cancellationToken) ?? 0;

        var adsRevenue = await query
            .Where(p => p.Type == PaymentType.Ads
                && (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.PartiallyRefunded))
            .SumAsync(p => (decimal?)(p.Amount - p.RefundedAmount), cancellationToken) ?? 0;

        var pendingCount = await query
            .CountAsync(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing, cancellationToken);

        // A partially refunded payment still succeeded.
        var successCount = await query.CountAsync(p => p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.PartiallyRefunded, cancellationToken);
        var failedCount = await query.CountAsync(p => p.Status == PaymentStatus.Failed, cancellationToken);
        var refundedCount = await query.CountAsync(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded, cancellationToken);

        return new AdminPaymentStatsDto
        {
            TodayRevenue = todayRevenue,
            AppointmentsRevenue = appointmentsRevenue,
            AppointmentsPlatformFees = appointmentsFees,
            AppointmentsNetRevenue = appointmentsNet,
            SubscriptionsRevenue = subscriptionsRevenue,
            AdsRevenue = adsRevenue,
            PendingCount = pendingCount,
            SuccessCount = successCount,
            FailedCount = failedCount,
            RefundedCount = refundedCount
        };
    }

    private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
            .GetAllAsync(s => !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return setting?.AppointmentFeePercent ?? 0m;
    }
}
