using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using ClinicHub.Domain.Common;

namespace ClinicHub.Application.Features.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppointmentCancellationService _cancellationService;
        private readonly IFcmService _fcmService;
        private readonly ILogger<CancelAppointmentCommandHandler> _logger;

        public CancelAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAppointmentCancellationService cancellationService,
            IFcmService fcmService,
            ILogger<CancelAppointmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _cancellationService = cancellationService;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task<bool> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            // Load appointment including Clinic so appointment.Clinic?.Name is available for notifications
            var appointment = await _unitOfWork.AppointmentRepository
                .GetFirstWithIncluding(
                    a => a.Id == request.AppointmentId,
                    a => a.Clinic!,
                    a => a.Doctor)
                .FirstOrDefaultAsync(cancellationToken);

            if (appointment == null)
                throw new NotFoundException(nameof(Appointment), request.AppointmentId);

            if (appointment.BookedByUserId != _currentUserService.UserId)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToCancel.Value);

            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.NoShow || appointment.Status == AppointmentStatus.Rejected)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotCancelAppointment.Value);

            var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);
            if (bookingConfig != null)
            {
                var payment = appointment.PaymentId.HasValue
                    ? await _unitOfWork.PaymentRepository
                        .GetAllAsync(p => p.Id == appointment.PaymentId.Value)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken)
                    : null;

                // A paid appointment is always refundable: the money was already captured and
                // the refund flow below returns it to the patient. The cancellation window
                // only protects UNPAID reservations, measured from the booking time.
                if (payment is null || payment.Status != PaymentStatus.Paid)
                {
                    var cancelDeadline = appointment.CreatedAt.AddMinutes(bookingConfig.CancellationWindowMinutes);
                    if (BaseEntity.AppNow > cancelDeadline)
                        throw new BadRequestException(LocalizationKeys.BookingMessages.CancellationWindowExpired.Value);
                }
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _cancellationService.CancelAsync(appointment, request.CancellationReason, cancellationToken);

                await _unitOfWork.CommitAsync();

                try
                {
                    await _fcmService.SendToUserAsync(appointment.BookedByUserId, NotificationType.AppointmentCancellation, new()
                    {
                        ["clinicName"] = appointment.Clinic?.Name ?? "",
                        ["reason"] = request.CancellationReason
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation notification for appointment {AppointmentId}.", appointment.Id);
                }

                try
                {
                    var recipients = new HashSet<Guid>();
                    if (appointment.Doctor != null)
                        recipients.Add(appointment.Doctor.UserId);
                    if (appointment.Clinic?.ClinicAdminId.HasValue == true)
                        recipients.Add(appointment.Clinic.ClinicAdminId.Value);
                    recipients.Remove(appointment.BookedByUserId);

                    foreach (var userId in recipients)
                    {
                        await _fcmService.SendToUserAsync(userId, NotificationType.AppointmentCancellation, new()
                        {
                            ["clinicName"] = appointment.Clinic?.Name ?? "",
                            ["reason"] = $"Patient {appointment.PatientFullName} cancelled: {request.CancellationReason}",
                            ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                            ["appointmentId"] = appointment.Id.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send clinic-side cancellation notification for appointment {AppointmentId}.", appointment.Id);
                }

                try
                {
                    // Persist the notification row added by the send above. The cancellation
                    // and refund are already committed — a notification-save hiccup must never
                    // surface as an error for an operation that actually succeeded.
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to persist cancellation notification row for appointment {AppointmentId}.", appointment.Id);
                }

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
