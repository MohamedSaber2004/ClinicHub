using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctorSchedule
{
    public class GetStaffDoctorScheduleQueryHandler : IRequestHandler<GetStaffDoctorScheduleQuery, DoctorScheduleDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffDoctorScheduleQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<DoctorScheduleDto> Handle(GetStaffDoctorScheduleQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;

            var doctor = await _unitOfWork.DoctorRepository
                .GetAllWithIncluding(
                    d => d.Id == request.DoctorId && !d.IsDeleted,
                    d => d.User,
                    d => d.Specialization)
                .FirstOrDefaultAsync(cancellationToken);

            if (doctor == null)
                return new DoctorScheduleDto();

            var targetDate = (request.Date ?? DateTime.Today).Date;
            var todayEnd = targetDate.AddDays(1);

            var appointments = await _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.ClinicId == clinicId
                        && a.DoctorId == request.DoctorId
                        && !a.IsDeleted
                        && a.AppointmentDate >= targetDate && a.AppointmentDate < todayEnd,
                    a => a.BookedByUser)
                .OrderBy(a => a.StartTime)
                .ToListAsync(cancellationToken);

            return new DoctorScheduleDto
            {
                Doctor = new DoctorBriefDto
                {
                    Id = doctor.Id,
                    Name = "د. " + (doctor.User?.FullName ?? ""),
                    Specialty = doctor.Specialization?.ArName ?? doctor.Specialization?.Name ?? ""
                },
                Date = targetDate.ToString("yyyy-MM-dd"),
                Appointments = appointments.Select(a =>
                {
                    var status = a.Status;
                    return new DoctorSlotDto
                    {
                        Patient = new PatientBriefDto
                        {
                            Id = a.BookedByUserId,
                            Name = a.PatientFullName,
                            Initial = StaffDashboardStatusHelper.GetInitial(a.PatientFullName)
                        },
                        Time = a.StartTime.ToString(@"hh\:mm"),
                        StatusLabel = StaffDashboardStatusHelper.GetStatusLabel(status),
                        StatusClass = StaffDashboardStatusHelper.GetStatusClass(status)
                    };
                }).ToList()
            };
        }
    }
}
