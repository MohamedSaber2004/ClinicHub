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

        var todayAmounts = await paid.Where(p => p.PaidAt >= today).Select(p => p.Amount).ToListAsync(cancellationToken);
        var monthAmounts = await paid.Where(p => p.PaidAt >= monthStart).Select(p => p.Amount).ToListAsync(cancellationToken);
        var paidAmounts = await paid.Select(p => p.Amount).ToListAsync(cancellationToken);
        var pendingAmounts = await query
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
            .Select(p => p.Amount).ToListAsync(cancellationToken);

        // Gross totals stay exact; net = total - superadmin platform fees (per-row split, then sum).
        var (todayFees, todayNet) = AppointmentRevenueSplitter.SumSplits(todayAmounts, percent);
        var (monthFees, monthNet) = AppointmentRevenueSplitter.SumSplits(monthAmounts, percent);
        var (paidFees, paidNet) = AppointmentRevenueSplitter.SumSplits(paidAmounts, percent);
        var (pendingFees, pendingNet) = AppointmentRevenueSplitter.SumSplits(pendingAmounts, percent);

        stats.TodayRevenue = todayAmounts.Sum();
        stats.TodayPlatformFees = todayFees;
        stats.TodayNetRevenue = todayNet;
        stats.MonthRevenue = monthAmounts.Sum();
        stats.MonthPlatformFees = monthFees;
        stats.MonthNetRevenue = monthNet;
        stats.PaidTotal = paidAmounts.Sum();
        stats.PaidPlatformFees = paidFees;
        stats.PaidNetTotal = paidNet;
        stats.PendingTotal = pendingAmounts.Sum();
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
