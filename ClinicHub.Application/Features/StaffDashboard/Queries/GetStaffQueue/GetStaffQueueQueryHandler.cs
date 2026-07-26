using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffQueue
{
    public class GetStaffQueueQueryHandler : IRequestHandler<GetStaffQueueQuery, PagginatedResult<StaffQueueItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffQueueQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<StaffQueueItemDto>> Handle(GetStaffQueueQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new PagginatedResult<StaffQueueItemDto>(Array.Empty<StaffQueueItemDto>(), 0);

            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var baseQuery = _unitOfWork.AppointmentRepository
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
                .ThenBy(a => a.CreatedAt);

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            var pageItems = await baseQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var offset = (request.PageNumber - 1) * request.PageSize;
            var dtos = pageItems.Select((a, i) =>
            {
                var status = a.Status;
                return new StaffQueueItemDto
                {
                    QueueNumber = offset + i + 1,
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
                    Status = StaffDashboardStatusHelper.GetQueueStatusValue(status),
                    StatusLabel = StaffDashboardStatusHelper.GetQueueStatusLabel(status),
                    StatusClass = StaffDashboardStatusHelper.GetQueueStatusClass(status)
                };
            }).ToList();

            return new PagginatedResult<StaffQueueItemDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
