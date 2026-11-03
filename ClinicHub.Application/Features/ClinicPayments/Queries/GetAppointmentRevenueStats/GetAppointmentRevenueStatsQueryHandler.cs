using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.ClinicPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.ClinicPayments.Queries.GetAppointmentRevenueStats;

public class GetAppointmentRevenueStatsQueryHandler
    : IRequestHandler<GetAppointmentRevenueStatsQuery, AppointmentRevenueStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAppointmentRevenueStatsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<AppointmentRevenueStatsDto> Handle(GetAppointmentRevenueStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = new AppointmentRevenueStatsDto();

        if (_currentUserService.CurrentClinicId is null)
            return stats;

        var today = DateTime.Now.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var query = _unitOfWork.PaymentRepository
            .GetAllAsync(null)
            .Where(p => p.Type == PaymentType.Appointment
                        && p.ClinicId == _currentUserService.CurrentClinicId.Value);

        var paid = query.Where(p => p.Status == PaymentStatus.Paid && p.PaidAt != null);

        var percent = await GetPlatformFeePercentAsync(cancellationToken);

        var todayRows = await paid.Where(p => p.PaidAt >= today)
            .Select(p => new { p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount })
            .ToListAsync(cancellationToken);
        var monthRows = await paid.Where(p => p.PaidAt >= monthStart)
            .Select(p => new { p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount })
            .ToListAsync(cancellationToken);
        var paidRows = await paid
            .Select(p => new { p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount })
            .ToListAsync(cancellationToken);
        var pendingRows = await query
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
            .Select(p => new { p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount })
            .ToListAsync(cancellationToken);

        // Gross totals stay exact; net uses the split captured at collection time, falling
        // back to the live percent only for legacy rows without a snapshot.
        var (todayFees, todayNet) = PaymentRevenueResolver.SumResolved(
            todayRows.Select(p => (p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount)), percent);
        var (monthFees, monthNet) = PaymentRevenueResolver.SumResolved(
            monthRows.Select(p => (p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount)), percent);
        var (paidFees, paidNet) = PaymentRevenueResolver.SumResolved(
            paidRows.Select(p => (p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount)), percent);
        var (pendingFees, pendingNet) = PaymentRevenueResolver.SumResolved(
            pendingRows.Select(p => (p.Amount, p.PlatformFeeAmount, p.ClinicNetAmount)), percent);

        stats.TodayRevenue = todayRows.Sum(p => p.Amount);
        stats.TodayPlatformFees = todayFees;
        stats.TodayNetRevenue = todayNet;
        stats.MonthRevenue = monthRows.Sum(p => p.Amount);
        stats.MonthPlatformFees = monthFees;
        stats.MonthNetRevenue = monthNet;
        stats.PaidTotal = paidRows.Sum(p => p.Amount);
        stats.PaidPlatformFees = paidFees;
        stats.PaidNetTotal = paidNet;
        stats.PendingTotal = pendingRows.Sum(p => p.Amount);
        stats.PendingPlatformFees = pendingFees;
        stats.PendingNetTotal = pendingNet;

        return stats;
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
