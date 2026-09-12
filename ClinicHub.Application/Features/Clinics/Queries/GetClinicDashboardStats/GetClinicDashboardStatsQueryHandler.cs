using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicDashboardStats
{
    public sealed class GetClinicDashboardStatsQueryHandler : IRequestHandler<GetClinicDashboardStatsQuery, ClinicDashboardStatsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetClinicDashboardStatsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicDashboardStatsDto> Handle(GetClinicDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;

            var now = DateTime.Now;
            var todayStart = now.Date;
            var todayEnd = todayStart.AddDays(1);

            var weekStart = todayStart.AddDays(-(((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7));
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            var appointmentsQuery = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.ClinicId == clinicId && !a.IsDeleted);

            var paymentsQuery = _unitOfWork.PaymentRepository
                .GetAllAsync(p => p.ClinicId == clinicId
                    && p.Type == PaymentType.Appointment
                    && p.Status == PaymentStatus.Paid
                    && p.PaidAt != null);

            var percent = await GetPlatformFeePercentAsync(cancellationToken);

            var todayVisits = await appointmentsQuery
                .CountAsync(a => a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd
                    && a.Status == AppointmentStatus.Completed, cancellationToken);

            var weeklyVisits = await appointmentsQuery
                .CountAsync(a => a.AppointmentDate >= weekStart && a.AppointmentDate < todayEnd
                    && a.Status == AppointmentStatus.Completed, cancellationToken);

            var monthlyVisits = await appointmentsQuery
                .CountAsync(a => a.AppointmentDate >= monthStart && a.AppointmentDate < todayEnd
                    && a.Status == AppointmentStatus.Completed, cancellationToken);

            var yearlyVisits = await appointmentsQuery
                .CountAsync(a => a.AppointmentDate >= yearStart && a.AppointmentDate < todayEnd
                    && a.Status == AppointmentStatus.Completed, cancellationToken);

            var todayAmounts = await paymentsQuery
                .Where(p => p.PaidAt >= todayStart && p.PaidAt < todayEnd)
                .Select(p => p.Amount)
                .ToListAsync(cancellationToken);
            var weeklyAmounts = await paymentsQuery
                .Where(p => p.PaidAt >= weekStart && p.PaidAt < todayEnd)
                .Select(p => p.Amount)
                .ToListAsync(cancellationToken);
            var monthlyAmounts = await paymentsQuery
                .Where(p => p.PaidAt >= monthStart && p.PaidAt < todayEnd)
                .Select(p => p.Amount)
                .ToListAsync(cancellationToken);
            var yearlyAmounts = await paymentsQuery
                .Where(p => p.PaidAt >= yearStart && p.PaidAt < todayEnd)
                .Select(p => p.Amount)
                .ToListAsync(cancellationToken);

            var todayIncome = todayAmounts.Sum();
            var (todayFees, todayNet) = AppointmentRevenueSplitter.SumSplits(todayAmounts, percent);
            var weeklyIncome = weeklyAmounts.Sum();
            var (weeklyFees, weeklyNet) = AppointmentRevenueSplitter.SumSplits(weeklyAmounts, percent);
            var monthlyIncome = monthlyAmounts.Sum();
            var (monthlyFees, monthlyNet) = AppointmentRevenueSplitter.SumSplits(monthlyAmounts, percent);
            var yearlyIncome = yearlyAmounts.Sum();
            var (yearlyFees, yearlyNet) = AppointmentRevenueSplitter.SumSplits(yearlyAmounts, percent);

            var pendingActions = await appointmentsQuery
                .CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken);

            return new ClinicDashboardStatsDto
            {
                TodayVisits = todayVisits,
                TodayIncome = todayIncome,
                TodayPlatformFees = todayFees,
                TodayNetIncome = todayNet,
                WeeklyVisits = weeklyVisits,
                WeeklyIncome = weeklyIncome,
                WeeklyPlatformFees = weeklyFees,
                WeeklyNetIncome = weeklyNet,
                MonthlyVisits = monthlyVisits,
                MonthlyIncome = monthlyIncome,
                MonthlyPlatformFees = monthlyFees,
                MonthlyNetIncome = monthlyNet,
                YearlyVisits = yearlyVisits,
                YearlyIncome = yearlyIncome,
                YearlyPlatformFees = yearlyFees,
                YearlyNetIncome = yearlyNet,
                PendingActions = pendingActions
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
}
