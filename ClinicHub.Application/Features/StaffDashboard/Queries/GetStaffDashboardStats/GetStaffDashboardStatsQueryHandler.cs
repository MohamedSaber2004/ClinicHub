using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDashboardStats
{
    public class GetStaffDashboardStatsQueryHandler : IRequestHandler<GetStaffDashboardStatsQuery, StaffDashboardStatsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffDashboardStatsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<StaffDashboardStatsDto> Handle(GetStaffDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new StaffDashboardStatsDto();

            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var todayAppointments = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.ClinicId == clinicId && !a.IsDeleted
                    && a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd);

            var total = await todayAppointments.CountAsync(cancellationToken);
            var checkedIn = await todayAppointments.CountAsync(a =>
                a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Accepted, cancellationToken);
            var waiting = await todayAppointments.CountAsync(a =>
                a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Reserved, cancellationToken);
            var completed = await todayAppointments.CountAsync(a =>
                a.Status == AppointmentStatus.Completed, cancellationToken);

            return new StaffDashboardStatsDto
            {
                TotalAppointments = total,
                CheckedIn = checkedIn,
                Waiting = waiting,
                Completed = completed
            };
        }
    }
}
