using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorDashboardStats
{
    public class GetDoctorDashboardStatsQueryHandler : IRequestHandler<GetDoctorDashboardStatsQuery, DoctorDashboardStatsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetDoctorDashboardStatsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<DoctorDashboardStatsDto> Handle(GetDoctorDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                return new DoctorDashboardStatsDto();

            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek + (int)DayOfWeek.Saturday);

            var todayAppointments = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted
                    && a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd);

            var todayTotal = await todayAppointments.CountAsync(cancellationToken);
            var pending = await todayAppointments.CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken);
            var accepted = await todayAppointments.CountAsync(a => a.Status == AppointmentStatus.Accepted, cancellationToken);
            var completed = await todayAppointments.CountAsync(a => a.Status == AppointmentStatus.Completed, cancellationToken);
            var cancelled = await todayAppointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled
                || a.Status == AppointmentStatus.Rejected, cancellationToken);

            // All-time stats
            var allAppointments = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted);

            var allTimeCompleted = await allAppointments.CountAsync(a => a.Status == AppointmentStatus.Completed, cancellationToken);
            var totalPatients = await allAppointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Select(a => a.BookedByUserId)
                .Distinct()
                .CountAsync(cancellationToken);

            var weekPatients = await _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted
                    && a.AppointmentDate >= weekStart && a.AppointmentDate < todayEnd
                    && a.Status == AppointmentStatus.Completed)
                .Select(a => a.BookedByUserId)
                .Distinct()
                .CountAsync(cancellationToken);

            var nextAppointment = await _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.DoctorId == doctor.Id && !a.IsDeleted
                        && a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd
                        && (a.Status == AppointmentStatus.Accepted || a.Status == AppointmentStatus.Pending),
                    a => a.Clinic)
                .OrderBy(a => a.StartTime)
                .FirstOrDefaultAsync(cancellationToken);

            AppointmentDto? nextDto = null;
            if (nextAppointment != null)
                nextDto = _mapper.Map<AppointmentDto>(nextAppointment);

            return new DoctorDashboardStatsDto
            {
                TodayAppointmentsCount = todayTotal,
                TotalPatientsCount = totalPatients,
                PendingAppointmentsCount = pending,
                CompletedAppointmentsCount = allTimeCompleted,
                AcceptedAppointments = accepted,
                CancelledAppointments = cancelled,
                TotalPatientsThisWeek = weekPatients,
                NextAppointment = nextDto
            };
        }
    }
}
