using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Appointments.Queries.GetMyAppointments
{
    /// <summary>
    /// Patient-facing "My appointments" endpoint: returns the current user's booking requests
    /// with their payment info (used by the mobile app, see appointment-request-payment-flow.md).
    /// </summary>
    public class GetMyAppointmentsQueryHandler : IRequestHandler<GetMyAppointmentsQuery, List<MyAppointmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyAppointmentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<MyAppointmentDto>> Handle(GetMyAppointmentsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var query = _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(
                    a => a.BookedByUserId == userId,
                    a => a.Clinic,
                    a => a.Doctor,
                    a => a.Doctor.User,
                    a => a.Payment);

            if (request.Status.HasValue)
                query = query.Where(a => a.Status == request.Status.Value);

            query = query.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.StartTime);

            var appointments = await query.ToListAsync(cancellationToken);

            return appointments.Select(a =>
            {
                var awaitingPayment = a.Status == AppointmentStatus.Accepted;
                return new MyAppointmentDto
                {
                    Id = a.Id,
                    ClinicId = a.ClinicId,
                    ClinicName = a.Clinic?.Name,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor?.User != null ? "د. " + a.Doctor.User.FullName : null,
                    Date = a.AppointmentDate.ToString("yyyy-MM-dd"),
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    Status = a.Status.ToString(),
                    RejectionReason = a.CancellationReason,
                    Payment = a.Payment == null ? null : new MyAppointmentPaymentDto
                    {
                        PaymentId = a.Payment.Id,
                        Amount = a.Payment.Amount,
                        Currency = a.Payment.Currency,
                        PaymentStatus = a.Payment.Status,
                        PaymobRedirectUrl = awaitingPayment ? a.Payment.RedirectUrl : null
                    }
                };
            }).ToList();
        }
    }
}
