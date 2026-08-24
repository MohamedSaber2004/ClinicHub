using ClinicHub.Application.Features.Users.Queries.GetUserAdminOverview;
using ClinicHub.Application.Features.AdminPayments;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Users.Queries.GetUserAdminOverview
{
    public class GetUserAdminOverviewQueryHandler
        : IRequestHandler<GetUserAdminOverviewQuery, AdminUserOverviewDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserAdminOverviewQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminUserOverviewDto> Handle(GetUserAdminOverviewQuery request, CancellationToken cancellationToken)
        {
            var dto = new AdminUserOverviewDto { Id = request.UserId };

            var user = await _unitOfWork.GetRepository<ApplicationUser, Guid>()
                .GetAllAsync(u => u.Id == request.UserId)
                .Select(u => new { u.Id, u.FullName, u.Email, u.PhoneNumber, u.IsActive, u.CreatedAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
                return dto;

            dto.FullName = user.FullName;
            dto.Email = user.Email ?? "";
            dto.Phone = user.PhoneNumber ?? "";
            dto.IsActive = user.IsActive;
            dto.CreatedAt = user.CreatedAt;

            var appointments = await _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.BookedByUserId == request.UserId && !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.StartTime,
                    a.Status,
                    DoctorName = a.Doctor.User.FullName,
                    Specialty = a.Doctor.Specialization.Name
                })
                .ToListAsync(cancellationToken);

            dto.TotalAppointments = appointments.Count;
            dto.TotalVisits = appointments.Count(a => a.Status == AppointmentStatus.Completed);
            dto.RecentVisits = appointments.Take(10).Select(a => new AdminUserVisitDto
            {
                AppointmentId = a.Id,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                DoctorName = a.DoctorName,
                Specialty = a.Specialty,
                Status = (int)a.Status
            }).ToList();

            var ratingsQuery = _unitOfWork.GetRepository<Rating, Guid>()
                .GetAllAsync(r => r.UserId == request.UserId && !r.IsDeleted);

            dto.ReviewCount = await ratingsQuery.CountAsync(cancellationToken);
            dto.AvgRating = dto.ReviewCount == 0
                ? null
                : Math.Round(await ratingsQuery.AverageAsync(r => (double)r.Value, cancellationToken), 1);

            var payments = await _unitOfWork.PaymentRepository
                .GetAllAsync(p => p.UserId == request.UserId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .Select(p => new
                {
                    p.Id,
                    p.Amount,
                    p.Currency,
                    p.Type,
                    p.Status,
                    p.PaymentMethod,
                    p.CreatedAt
                })
                .ToListAsync(cancellationToken);

            dto.TotalSpent = payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .Sum(p => p.Amount);
            dto.Payments = payments.Select(p => new AdminUserPaymentDto
            {
                PaymentId = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Type = (int)p.Type,
                Status = (int)PaymentMethodMapper.ToUiStatus(p.Status),
                Method = (int)PaymentMethodMapper.ToEnum(p.PaymentMethod),
                CreatedAt = p.CreatedAt
            }).ToList();

            dto.Requests = await _unitOfWork.GetRepository<UserVerification, Guid>()
                .GetAllAsync(v => v.UserId == request.UserId && !v.IsDeleted)
                .OrderByDescending(v => v.RequestedAt)
                .Select(v => new AdminUserRequestDto
                {
                    RequestId = v.Id,
                    RequestedRole = (int)v.RequestedRole,
                    Status = (int)v.Status,
                    RequestedAt = v.RequestedAt,
                    ReviewedAt = v.ReviewedAt
                })
                .ToListAsync(cancellationToken);

            return dto;
        }
    }
}
