using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;

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
            // Load appointment including Clinic so appointment.Clinic?.Name is available for notifications
            var appointment = await _unitOfWork.AppointmentRepository
                .GetFirstWithIncluding(
                    a => a.Id == request.AppointmentId,
                    a => a.Clinic!)
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
                    if (DateTime.Now > cancelDeadline)
                        throw new BadRequestException(LocalizationKeys.BookingMessages.CancellationWindowExpired.Value);
                }
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (appointment.PaymentId.HasValue)
                {
                    var paymentId = appointment.PaymentId.Value;

                    await PaymentRefundGate.RunAsync(paymentId, async () =>
                    {
                        var payment = await _unitOfWork.PaymentRepository
                            .GetAllAsync(p => p.Id == paymentId)
                            .FirstOrDefaultAsync(cancellationToken);

                        // No payment record, or already refunded (by this request, the retry
                        // job, or an admin) — nothing left to refund.
                        if (payment is null || payment.Status == PaymentStatus.Refunded)
                            return;

                        if (payment.Status == PaymentStatus.Paid)
                        {
                            // The VerifyBookingPayment path stores the transaction id in
                            // TransactionId only — either one is a valid Paymob refund key.
                            var paymobTransactionId = payment.PaymobTransactionId ??
                                (long.TryParse(payment.TransactionId, out _) ? payment.TransactionId : null);

                            if (!string.IsNullOrEmpty(paymobTransactionId))
                            {
                                RefundResultDto? refundResult;

                                try
                                {
                                    // Use CancellationToken.None: a client disconnect must never abort
                                    // the refund mid-flight. Timeouts are governed by the Paymob
                                    // HttpClient's own timeout.
                                    refundResult = await _paymobService.RefundTransactionAsync(
                                        paymobTransactionId,
                                        payment.Amount,
                                        CancellationToken.None);
                                }
                                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                                {
                                    // Paymob is unreachable, timed out, or returned an unparseable
                                    // response — never turn a transient transport failure into a 500.
                                    // The refund is retried in the background by RefundRetryJob.
                                    _logger.LogWarning(ex, "Paymob refund call failed for payment {PaymentId}; refund scheduled for background retry.", payment.Id);
                                    refundResult = null;
                                }

                                if (refundResult == null || !refundResult.Success)
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
                            else
                            {
                                payment.MarkAsRefunded("Cancelled by user (No Paymob transaction ID)");
                            }
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
