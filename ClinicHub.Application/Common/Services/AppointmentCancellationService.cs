using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace ClinicHub.Application.Common.Services
{
    /// <summary>
    /// Shared implementation of the appointment cancellation money path: the captured payment
    /// is returned to the patient in full and the clinic's accrued earning is reversed.
    /// A gateway failure does not block the cancellation: the refund is retried in the background.
    /// Authorization and notifications belong to the caller, and the gateway call ignores the
    /// request cancellation token so a client disconnect cannot abort a refund mid-flight.
    /// </summary>
    public class AppointmentCancellationService : IAppointmentCancellationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymobService _paymobService;
        private readonly IClinicLedgerService _clinicLedgerService;
        private readonly IBackgroundJobScheduler _jobScheduler;
        private readonly ILogger<AppointmentCancellationService> _logger;

        public AppointmentCancellationService(
            IUnitOfWork unitOfWork,
            IPaymobService paymobService,
            IClinicLedgerService clinicLedgerService,
            IBackgroundJobScheduler jobScheduler,
            ILogger<AppointmentCancellationService> logger)
        {
            _unitOfWork = unitOfWork;
            _paymobService = paymobService;
            _clinicLedgerService = clinicLedgerService;
            _jobScheduler = jobScheduler;
            _logger = logger;
        }

        public async Task CancelAsync(Appointment appointment, string reason, CancellationToken cancellationToken)
        {
            if (appointment.PaymentId.HasValue)
                await RefundAsync(appointment.PaymentId.Value, cancellationToken);

            appointment.Cancel(reason);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task RefundAsync(Guid paymentId, CancellationToken cancellationToken)
        {
            await PaymentRefundGate.RunAsync(paymentId, async () =>
            {
                var payment = await _unitOfWork.PaymentRepository
                    .GetAllAsync(p => p.Id == paymentId)
                    .FirstOrDefaultAsync(cancellationToken);

                // No payment record, or already refunded (by this request, the retry
                // job, or an admin) — nothing left to refund.
                if (payment is null || payment.Status != PaymentStatus.Paid)
                    return;

                // The VerifyBookingPayment path stores the transaction id in
                // TransactionId only — either one is a valid Paymob refund key.
                var paymobTransactionId = payment.PaymobTransactionId ??
                    (long.TryParse(payment.TransactionId, out _) ? payment.TransactionId : null);

                if (string.IsNullOrEmpty(paymobTransactionId))
                {
                    await MarkRefundedAsync(payment, "Appointment cancelled (No Paymob transaction ID)", cancellationToken);
                    return;
                }

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
                    return;
                }

                await MarkRefundedAsync(payment, "Appointment cancelled", cancellationToken);
            });
        }

        private async Task MarkRefundedAsync(Payment payment, string reason, CancellationToken cancellationToken)
        {
            payment.MarkAsRefunded(reason);
            _unitOfWork.PaymentRepository.Update(payment);

            // The clinic was credited for a payment the patient now has back, so the same
            // net comes straight out of its payoutable balance.
            await _clinicLedgerService.DeductRefundedEarningAsync(payment, cancellationToken);
        }
    }
}
