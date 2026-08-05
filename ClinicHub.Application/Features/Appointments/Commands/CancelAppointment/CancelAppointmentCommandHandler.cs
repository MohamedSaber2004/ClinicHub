using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPaymobService _paymobService;
        private readonly IFcmService _fcmService;
        private readonly IBackgroundJobScheduler _jobScheduler;
        private readonly ILogger<CancelAppointmentCommandHandler> _logger;

        public CancelAppointmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IPaymobService paymobService,
            IFcmService fcmService,
            IBackgroundJobScheduler jobScheduler,
            ILogger<CancelAppointmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _paymobService = paymobService;
            _fcmService = fcmService;
            _jobScheduler = jobScheduler;
            _logger = logger;
        }

        public async Task<bool> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment.BookedByUserId != _currentUserService.UserId)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.NotAuthorizedToCancel.Value);

            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.NoShow)
                throw new BadRequestException(LocalizationKeys.AppointmentMessages.CannotCancelAppointment.Value);

            var bookingConfig = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(appointment.ClinicId);
            if (bookingConfig != null)
            {
                var payment = appointment.PaymentId.HasValue
                    ? await _unitOfWork.PaymentRepository.GetByIdAsync(appointment.PaymentId.Value)
                    : null;

                // Cancellation window is measured from the payment time (PaidAt) for paid appointments,
                // and from creation time for unpaid ones. After it passes, cancellation is blocked entirely.
                var cancelDeadline = (payment?.PaidAt ?? appointment.CreatedAt.ToUniversalTime()).AddMinutes(bookingConfig.CancellationWindowMinutes);
                if (DateTime.UtcNow > cancelDeadline)
                    throw new BadRequestException(LocalizationKeys.BookingMessages.CancellationWindowExpired.Value);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (appointment.PaymentId.HasValue)
                {
                    var paymentId = appointment.PaymentId.Value;

                    await PaymentRefundGate.RunAsync(paymentId, async () =>
                    {
                        var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
                        if (payment != null && payment.Status == PaymentStatus.Paid)
                        {
                            // Another concurrent request may have refunded this payment
                            // while we were waiting for the gate.
                            var alreadyRefunded = await _unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
                                .AsNoTracking()
                                .AnyAsync(p => p.Status == PaymentStatus.Refunded, cancellationToken);
                            if (alreadyRefunded)
                                return;

                            var refundResult = await _paymobService.RefundTransactionAsync(
                                payment.PaymobTransactionId!,
                                payment.Amount,
                                cancellationToken);

                            if (!refundResult.Success)
                            {
                                // Never block the cancellation on a transient Paymob failure —
                                // the refund is retried in the background by RefundRetryJob.
                                await _jobScheduler.ScheduleRefundRetryAsync(payment.Id);
                            }
                            else
                            {
                                payment.MarkAsRefunded("Cancelled by user");
                            }
                        }
                        else if (payment != null && payment.Status == PaymentStatus.Refunded)
                        {
                            throw new BadRequestException(LocalizationKeys.PaymentMessages.AlreadyRefunded.Value);
                        }
                    });
                }

                appointment.Cancel(request.CancellationReason);
                var result = await _unitOfWork.SaveChangesAsync();

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

                return result > 0;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
