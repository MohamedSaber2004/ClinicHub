using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicBookings
{
    public sealed class GetClinicBookingsQueryHandler : IRequestHandler<GetClinicBookingsQuery, PagginatedResult<ClinicBookingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClinicBookingsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagginatedResult<ClinicBookingDto>> Handle(GetClinicBookingsQuery request, CancellationToken cancellationToken)
        {
            var statuses = ParseStatuses(request.Status);

            var query = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => statuses.Contains(a.Status))
                .OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new
                {
                    a.Id,
                    a.PatientFullName,
                    a.PatientPhoneNumber,
                    a.PatientAge,
                    a.Complaint,
                    a.AppointmentDate,
                    a.StartTime,
                    a.AppointmentType,
                    a.Status,
                    a.CreatedAt,
                    ClinicName = a.Clinic != null ? a.Clinic.Name : null,
                    DoctorName = a.Doctor != null && a.Doctor.User != null ? a.Doctor.User.FullName : null
                })
                .ToListAsync(cancellationToken);

            var dtos = items.Select(i => new ClinicBookingDto
            {
                Id = i.Id,
                PatientName = i.PatientFullName,
                PatientPhone = i.PatientPhoneNumber,
                PatientAge = i.PatientAge,
                Reason = i.Complaint,
                ClinicName = i.ClinicName ?? "",
                DoctorName = i.DoctorName ?? "",
                RequestedDate = i.AppointmentDate.ToString("yyyy-MM-dd"),
                RequestedTime = i.StartTime.ToString(@"hh\:mm tt"),
                AppointmentType = i.AppointmentType == AppointmentType.Examination ? "inPerson" : "followUp",
                Status = i.Status switch
                {
                    AppointmentStatus.Pending => "pending",
                    AppointmentStatus.Accepted => "accepted",
                    AppointmentStatus.Rejected => "rejected",
                    _ => "pending"
                },
                CreatedAt = i.CreatedAt
            }).ToList();

            return new PagginatedResult<ClinicBookingDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }

        private static List<AppointmentStatus> ParseStatuses(string? statusFilter)
        {
            if (string.IsNullOrWhiteSpace(statusFilter))
                return [AppointmentStatus.Pending, AppointmentStatus.Accepted, AppointmentStatus.Rejected];

            return statusFilter.ToLower() switch
            {
                "pending" => [AppointmentStatus.Pending],
                "accepted" => [AppointmentStatus.Accepted],
                "rejected" => [AppointmentStatus.Rejected],
                _ => throw new BadRequestException($"Invalid booking status '{statusFilter}'. Valid values: pending, accepted, rejected.")
            };
        }
    }
}
