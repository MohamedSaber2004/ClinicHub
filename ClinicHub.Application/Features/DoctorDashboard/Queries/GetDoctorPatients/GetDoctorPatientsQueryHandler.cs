using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.DoctorDashboard.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorPatients
{
    public class GetDoctorPatientsQueryHandler : IRequestHandler<GetDoctorPatientsQuery, PagginatedResult<DoctorPatientDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetDoctorPatientsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<DoctorPatientDto>> Handle(GetDoctorPatientsQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUserService.UserId && !d.IsDeleted, cancellationToken);

            if (doctor == null)
                return new PagginatedResult<DoctorPatientDto>(Array.Empty<DoctorPatientDto>(), 0);

            var patientQuery = _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.DoctorId == doctor.Id && !a.IsDeleted && a.Status == AppointmentStatus.Completed,
                    a => a.BookedByUser)
                .GroupBy(a => a.BookedByUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    FullName = g.First().BookedByUser.FullName,
                    PhoneNumber = g.First().BookedByUser.PhoneNumber,
                    BirthDate = g.First().BookedByUser.BirthDate,
                    Gender = g.First().BookedByUser.Gender,
                    TotalVisits = g.Count(),
                    LastVisitDate = g.Max(a => a.AppointmentDate)
                });

            if (!string.IsNullOrWhiteSpace(request.Search))
                patientQuery = patientQuery.Where(p => p.FullName.Contains(request.Search)
                    || (p.PhoneNumber != null && p.PhoneNumber.Contains(request.Search)));

            var totalCount = await patientQuery.CountAsync(cancellationToken);

            var items = await patientQuery
                .OrderByDescending(p => p.LastVisitDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(p => new DoctorPatientDto
            {
                UserId = p.UserId,
                FullName = p.FullName,
                PhoneNumber = p.PhoneNumber,
                Age = p.BirthDate.HasValue ? (int?)(DateTime.Today.Year - p.BirthDate.Value.Year) : null,
                Gender = p.Gender,
                TotalVisits = p.TotalVisits,
                LastVisitDate = p.LastVisitDate
            }).ToList();

            return new PagginatedResult<DoctorPatientDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
