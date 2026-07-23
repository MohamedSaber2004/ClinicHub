using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffAppointments
{
    public class GetStaffAppointmentsQueryHandler : IRequestHandler<GetStaffAppointmentsQuery, PagginatedResult<StaffAppointmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffAppointmentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<StaffAppointmentDto>> Handle(GetStaffAppointmentsQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new PagginatedResult<StaffAppointmentDto>(Array.Empty<StaffAppointmentDto>(), 0);

            var query = _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.ClinicId == clinicId && !a.IsDeleted,
                    a => a.Doctor,
                    a => a.Doctor.User,
                    a => a.BookedByUser)
                .AsQueryable();

            if (request.Status.HasValue)
                query = query.Where(a => a.Status == request.Status.Value);

            var targetDate = request.Date?.Date ?? DateTime.Today;
            query = query.Where(a => a.AppointmentDate == targetDate);

            if (!string.IsNullOrWhiteSpace(request.PatientName))
                query = query.Where(a => a.PatientFullName.Contains(request.PatientName)
                    || a.BookedByUser.FullName.Contains(request.PatientName));

            query = query.OrderBy(a => a.StartTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(a => new StaffAppointmentDto
            {
                Id = a.Id,
                DoctorName = a.Doctor?.User?.FullName,
                BookedByUserName = a.BookedByUser.FullName,
                AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                StartTime = a.StartTime.ToString(@"hh\:mm"),
                EndTime = a.EndTime.ToString(@"hh\:mm"),
                AppointmentType = a.AppointmentType,
                Status = a.Status,
                PatientFullName = a.PatientFullName,
                PatientPhoneNumber = a.PatientPhoneNumber,
                PatientAge = a.PatientAge,
                PatientGender = a.PatientGender,
                Complaint = a.Complaint,
                CancellationReason = a.CancellationReason,
                CreatedAt = a.CreatedAt
            }).ToList();

            return new PagginatedResult<StaffAppointmentDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
