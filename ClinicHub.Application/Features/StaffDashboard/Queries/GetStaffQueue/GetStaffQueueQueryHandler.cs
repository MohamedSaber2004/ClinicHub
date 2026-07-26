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
                        && (a.Status == AppointmentStatus.Reserved
                            || a.Status == AppointmentStatus.Accepted
                            || a.Status == AppointmentStatus.Confirmed
                            || a.Status == AppointmentStatus.Completed),
                    a => a.Doctor,
                    a => a.Doctor.User,
                    a => a.Doctor.Specialization)
                .OrderBy(a => a.StartTime)
                .ThenBy(a => a.CreatedAt)
                .ToListAsync(cancellationToken);

            var queueNumber = 1;
            return queueItems.Select(a =>
            {
                var status = a.Status;
                var statusValue = StaffDashboardStatusHelper.GetQueueStatusValue(status);
                var statusLabel = StaffDashboardStatusHelper.GetQueueStatusLabel(status);
                var statusClass = StaffDashboardStatusHelper.GetQueueStatusClass(status);

                return new StaffQueueItemDto
                {
                    QueueNumber = queueNumber++,
                    Patient = new PatientBriefDto
                    {
                        Id = a.BookedByUserId,
                        Name = a.PatientFullName,
                        Initial = StaffDashboardStatusHelper.GetInitial(a.PatientFullName)
                    },
                    Doctor = new DoctorBriefDto
                    {
                        Id = a.Doctor.Id,
                        Name = "د. " + (a.Doctor.User?.FullName ?? ""),
                        Specialty = a.Doctor.Specialization?.ArName ?? a.Doctor.Specialization?.Name ?? ""
                    },
                    Time = a.StartTime.ToString(@"hh\:mm"),
                    Status = statusValue,
                    StatusLabel = statusLabel,
                    StatusClass = statusClass
                };
            }).ToList();
        }
    }
}
