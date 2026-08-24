using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
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

            var todayIncome = await paymentsQuery
                .Where(p => p.PaidAt >= todayStart && p.PaidAt < todayEnd)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

            var weeklyIncome = await paymentsQuery
                .Where(p => p.PaidAt >= weekStart && p.PaidAt < todayEnd)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

            var monthlyIncome = await paymentsQuery
                .Where(p => p.PaidAt >= monthStart && p.PaidAt < todayEnd)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

            var yearlyIncome = await paymentsQuery
                .Where(p => p.PaidAt >= yearStart && p.PaidAt < todayEnd)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

            var pendingActions = await appointmentsQuery
                .CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken);

            return new ClinicDashboardStatsDto
            {
                TodayVisits = todayVisits,
                TodayIncome = todayIncome,
                WeeklyVisits = weeklyVisits,
                WeeklyIncome = weeklyIncome,
                MonthlyVisits = monthlyVisits,
                MonthlyIncome = monthlyIncome,
                YearlyVisits = yearlyVisits,
                YearlyIncome = yearlyIncome,
                PendingActions = pendingActions
            };
        }
    }
}
