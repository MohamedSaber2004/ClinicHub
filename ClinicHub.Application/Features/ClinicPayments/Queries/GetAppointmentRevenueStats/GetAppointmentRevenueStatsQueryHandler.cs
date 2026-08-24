using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.ClinicPayments.DTOs;
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

        stats.TodayRevenue = await paid.Where(p => p.PaidAt >= today).SumAsync(p => p.Amount, cancellationToken);
        stats.MonthRevenue = await paid.Where(p => p.PaidAt >= monthStart).SumAsync(p => p.Amount, cancellationToken);
        stats.PaidTotal = await paid.SumAsync(p => p.Amount, cancellationToken);
        stats.PendingTotal = await query
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
            .SumAsync(p => p.Amount, cancellationToken);

        return stats;
    }
}
