using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorAppointments
{
    public class GetDoctorAppointmentsQueryHandler : IRequestHandler<GetDoctorAppointmentsQuery, PagginatedResult<DoctorAppointmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetDoctorAppointmentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<DoctorAppointmentDto>> Handle(GetDoctorAppointmentsQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                return new PagginatedResult<DoctorAppointmentDto>(Array.Empty<DoctorAppointmentDto>(), 0);

            var query = _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.DoctorId == doctor.Id && !a.IsDeleted,
                    a => a.BookedByUser,
                    a => a.Clinic)
                .AsQueryable();

            if (request.Status.HasValue)
                query = query.Where(a => a.Status == request.Status.Value);

            if (request.StartDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= request.StartDate.Value.Date);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= request.EndDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(request.PatientName))
                query = query.Where(a => a.PatientFullName.Contains(request.PatientName)
                    || a.BookedByUser.FullName.Contains(request.PatientName));

            query = query.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.StartTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(a => new DoctorAppointmentDto
            {
                Id = a.Id,
                BookedByUserId = a.BookedByUserId,
                BookedByUserName = a.BookedByUser.FullName,
                BookedByUserPhone = a.BookedByUser.PhoneNumber,
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
                ChronicDiseases = a.ChronicDiseases,
                CancellationReason = a.CancellationReason,
                CreatedAt = a.CreatedAt,
                ClinicName = a.Clinic?.Name
            }).ToList();

            return new PagginatedResult<DoctorAppointmentDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
