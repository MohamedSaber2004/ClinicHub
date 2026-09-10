using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Application.Localization;
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

            // Optional status filter: names in any case ("Pending"), numbers ("0" is
            // Pending, "4" is Reserved), or "All"/empty for every status. Anything
            // else is a 400 with the valid values instead of a silent empty list.
            // NOTE: `?status=0` and `?status=Pending` resolve to the same value
            // (Pending) by construction, so they can never return different rows.
            // Booking requests always enter as Pending, so `?status=0` returns the
            // caller's new requests. An always-empty result means no row matches
            // (owner + status + global filters). Verify with:
            // SELECT "Status", COUNT(*) FROM "Appointments"
            // WHERE "BookedByUserId" = '<caller id>' GROUP BY "Status";
            // (Pending=0, Confirmed=1, Cancelled=2, Completed=3, Reserved=4,
            //  NoShow=5, Accepted=6, Rejected=7.)
            var statusFilter = ResolveStatusFilter(request.Status);

            if (statusFilter.HasValue)
                query = query.Where(a => a.Status == statusFilter.Value);

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

        private static AppointmentStatus? ResolveStatusFilter(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)
                || status.Equals("All", StringComparison.OrdinalIgnoreCase))
                return null;

            if (Enum.TryParse<AppointmentStatus>(status.Trim(), ignoreCase: true, out var parsed)
                && Enum.IsDefined(typeof(AppointmentStatus), parsed))
                return parsed;

            throw new BadRequestException(LocalizationKeys.BookingMessages.InvalidStatus.Value);
        }
    }
}
