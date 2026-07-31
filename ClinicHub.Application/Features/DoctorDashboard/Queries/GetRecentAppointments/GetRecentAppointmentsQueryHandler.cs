using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetRecentAppointments
{
    public class GetRecentAppointmentsQueryHandler
        : IRequestHandler<GetRecentAppointmentsQuery, IReadOnlyCollection<DoctorAppointmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetRecentAppointmentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<IReadOnlyCollection<DoctorAppointmentDto>> Handle(
            GetRecentAppointmentsQuery request,
            CancellationToken cancellationToken)
        {
            var limit = Math.Clamp(request.Limit, 1, 50);

            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                return Array.Empty<DoctorAppointmentDto>();

            var items = await _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.DoctorId == doctor.Id && !a.IsDeleted,
                    a => a.BookedByUser,
                    a => a.Clinic)
                .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.StartTime)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return items.Select(a => new DoctorAppointmentDto
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
            }).ToList().AsReadOnly();
        }
    }
}
