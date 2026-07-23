using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffQueue
{
    public class GetStaffQueueQueryHandler : IRequestHandler<GetStaffQueueQuery, List<StaffQueueItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffQueueQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<StaffQueueItemDto>> Handle(GetStaffQueueQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new List<StaffQueueItemDto>();

            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var queueItems = await _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.ClinicId == clinicId && !a.IsDeleted
                        && a.AppointmentDate >= todayStart && a.AppointmentDate < todayEnd
                        && (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Accepted),
                    a => a.Doctor,
                    a => a.Doctor.User)
                .OrderBy(a => a.StartTime)
                .ToListAsync(cancellationToken);

            var now = DateTime.Now;
            return queueItems.Select(a =>
            {
                var appointmentDateTime = a.AppointmentDate.Date + a.StartTime;
                var waitMinutes = appointmentDateTime > now ? (int)(appointmentDateTime - now).TotalMinutes : 0;

                return new StaffQueueItemDto
                {
                    AppointmentId = a.Id,
                    PatientFullName = a.PatientFullName,
                    DoctorName = a.Doctor?.User?.FullName,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    Status = a.Status,
                    WaitTimeMinutes = waitMinutes
                };
            }).ToList();
        }
    }
}
