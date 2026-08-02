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

        var today = DateTime.UtcNow.Date;

        var todayRevenue = await query
            .Where(p => p.Status == PaymentStatus.Paid
                && (hasDateRange || (p.PaidAt.HasValue && p.PaidAt.Value >= today)))
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var appointmentsRevenue = await query
            .Where(p => p.Type == PaymentType.Appointment && p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var subscriptionsRevenue = await query
            .Where(p => p.Type == PaymentType.Subscription && p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var adsRevenue = await query
            .Where(p => p.Type == PaymentType.Ads && p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var pendingCount = await query
            .CountAsync(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing, cancellationToken);

        var successCount = await query.CountAsync(p => p.Status == PaymentStatus.Paid, cancellationToken);
        var failedCount = await query.CountAsync(p => p.Status == PaymentStatus.Failed, cancellationToken);
        var refundedCount = await query.CountAsync(p => p.Status == PaymentStatus.Refunded, cancellationToken);

        return new AdminPaymentStatsDto
        {
            TodayRevenue = todayRevenue,
            AppointmentsRevenue = appointmentsRevenue,
            SubscriptionsRevenue = subscriptionsRevenue,
            AdsRevenue = adsRevenue,
            PendingCount = pendingCount,
            SuccessCount = successCount,
            FailedCount = failedCount,
            RefundedCount = refundedCount
        };
    }
}
