using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientHistory
{
    public class GetPatientHistoryQueryHandler : IRequestHandler<GetPatientHistoryQuery, PagginatedResult<PatientHistoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetPatientHistoryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<PatientHistoryDto>> Handle(GetPatientHistoryQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                return new PagginatedResult<PatientHistoryDto>(Array.Empty<PatientHistoryDto>(), 0);

            var query = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted
                    && a.BookedByUserId == request.PatientUserId)
                .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.StartTime);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(a => new PatientHistoryDto
            {
                AppointmentId = a.Id,
                AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                StartTime = a.StartTime.ToString(@"hh\:mm"),
                EndTime = a.EndTime.ToString(@"hh\:mm"),
                AppointmentType = a.AppointmentType,
                Status = a.Status,
                Complaint = a.Complaint,
                ChronicDiseases = a.ChronicDiseases,
                CancellationReason = a.CancellationReason
            }).ToList();

            return new PagginatedResult<PatientHistoryDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
