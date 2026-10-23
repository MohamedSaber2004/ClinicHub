using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Retries definite-failure refunds. Two clearly separated branches:
/// 1) Subscription pro-rated refunds (RefundStatus == Pending, frozen RefundAmount);
/// 2) Legacy appointment full refunds (Status == Paid, full Amount).
/// Never processes Processing or NeedsReview automatically — an ambiguous
/// outcome could already have moved money.
/// </summary>
public class RefundRetryJob
{
    private const int MaxAttempts = 4;
    private static readonly TimeSpan StaleProcessingThreshold = TimeSpan.FromMinutes(15);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefundRetryJob> _logger;

    public RefundRetryJob(IServiceProvider serviceProvider, ILogger<RefundRetryJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task RetryRefundAsync(Guid paymentId, int attempt, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var paymobService = scope.ServiceProvider.GetRequiredService<IPaymobService>();
        var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var payment = await unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Clinic)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
            return;

        // --- Stale Processing sweep -------------------------------------------------
        // A claim that never completed (process died between the claim save and the
        // Paymob response) is ambiguous. Flip it to NeedsReview; never auto-retry.
        if (payment.RefundStatus == PaymentRefundStatus.Processing
            && payment.RefundRequestedAt.HasValue
            && payment.RefundRequestedAt.Value < DateTime.UtcNow - StaleProcessingThreshold)
        {
            _logger.LogWarning("Stale Processing refund for payment {PaymentId}; marking NeedsReview.", paymentId);
            payment.RequireRefundReview("Refund stuck in Processing with no completion.");
            unitOfWork.PaymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();
            await NotifySuperAdminsAsync(userManager, fcmService, payment, "needsReview");
            return;
        }

        // --- Branch 1: subscription pro-rated refund --------------------------------
        if (payment.RefundStatus == PaymentRefundStatus.Pending)
        {
            await RetrySubscriptionRefundAsync(payment, unitOfWork, paymobService, fcmService, userManager, attempt, cancellationToken);
            return;
        }

        // NeedsReview / Processing / Completed / Failed are never auto-retried here.
        if (payment.RefundStatus != PaymentRefundStatus.None)
            return;

        // --- Branch 2: legacy appointment refund (unchanged behaviour) --------------
        if (payment.Status != PaymentStatus.Paid)
            return;

        await RetryLegacyRefundAsync(payment, unitOfWork, paymobService, fcmService, attempt, cancellationToken);
    }

    private async Task RetrySubscriptionRefundAsync(
        Payment payment,
        IUnitOfWork unitOfWork,
        IPaymobService paymobService,
        IFcmService fcmService,
        UserManager<ApplicationUser> userManager,
        int attempt,
        CancellationToken cancellationToken)
    {
        var paymentId = payment.Id;

        if (!long.TryParse(payment.PaymobTransactionId, out _))
        {
            _logger.LogWarning("Subscription refund retry skipped for payment {PaymentId}: missing Paymob transaction id.", paymentId);
            return;
        }

        var refundResult = await PaymentRefundGate.RunAsync(paymentId, async () =>
        {
            // Re-claim against the DB: another request may own this refund already.
            var fresh = await unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
                .FirstOrDefaultAsync(cancellationToken);
            if (fresh == null || fresh.RefundStatus != PaymentRefundStatus.Pending)
                return new RefundResultDto { Success = false, Message = "Refund not pending" };

            if (!fresh.ClaimRefund())
                return new RefundResultDto { Success = false, Message = "Claim failed" };

            unitOfWork.PaymentRepository.Update(fresh);
            await unitOfWork.SaveChangesAsync();

            // Use the frozen amount — never payment.Amount for a subscription refund.
            return await paymobService.RefundTransactionAsync(fresh.PaymobTransactionId!, fresh.RefundAmount, cancellationToken);
        });

        if (refundResult.Success)
        {
            payment.CompleteRefund(payment.RefundAmount, refundResult.RefundId ?? refundResult.Message);
            unitOfWork.PaymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Subscription refund retry succeeded for payment {PaymentId}.", paymentId);
            return;
        }

        if (attempt >= MaxAttempts)
        {
            _logger.LogError("Subscription refund retries exhausted for payment {PaymentId} after {MaxAttempts} attempts.", paymentId, MaxAttempts);
            payment.AbandonRefund($"Refund retries exhausted after {MaxAttempts} attempts.");
            unitOfWork.PaymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();
            await NotifySuperAdminsAsync(userManager, fcmService, payment, "failed");
            return;
        }

        // Definite failure → refund returns to Pending so the next retry can claim it.
        payment.FailRefund(refundResult.Message ?? "Refund failed");
        unitOfWork.PaymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync();

        var delay = TimeSpan.FromHours(Math.Pow(2, attempt));
        BackgroundJob.Schedule<RefundRetryJob>(job => job.RetryRefundAsync(paymentId, attempt + 1, CancellationToken.None), delay);
        _logger.LogWarning("Subscription refund retry {Attempt} failed for payment {PaymentId}; rescheduled in {Delay} hours.", attempt, paymentId, delay.TotalHours);
    }

    private async Task RetryLegacyRefundAsync(
        Payment payment,
        IUnitOfWork unitOfWork,
        IPaymobService paymobService,
        IFcmService fcmService,
        int attempt,
        CancellationToken cancellationToken)
    {
        var paymentId = payment.Id;

        // The VerifyBookingPayment path stores the transaction id in TransactionId only —
        // either one is a valid Paymob refund key.
        var paymobTransactionId = payment.PaymobTransactionId ??
            (long.TryParse(payment.TransactionId, out _) ? payment.TransactionId : null);

        if (string.IsNullOrWhiteSpace(paymobTransactionId))
        {
            _logger.LogWarning("Refund retry skipped for payment {PaymentId}: missing Paymob transaction id.", paymentId);
            return;
        }

        var refundResult = await PaymentRefundGate.RunAsync(paymentId, async () =>
        {
            // Re-check against the DB: the refund may have succeeded elsewhere
            // (user re-cancel, admin refund) while this retry was in flight.
            var alreadyRefunded = await unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
                .AsNoTracking()
                .AnyAsync(p => p.Status == PaymentStatus.Refunded, cancellationToken);
            if (alreadyRefunded)
                return new RefundResultDto { Success = true, Message = "Already refunded" };

            return await paymobService.RefundTransactionAsync(paymobTransactionId, payment.Amount, cancellationToken);
        });
        if (refundResult.Success)
        {
            payment.MarkAsRefunded("Refunded by background retry (user cancellation).");

            if (payment.AppointmentId.HasValue && payment.Appointment != null)
            {
                await fcmService.SendToUserAsync(payment.Appointment.BookedByUserId, NotificationType.RefundProcessed, new()
                {
                    ["clinicName"] = payment.Appointment.Clinic?.Name ?? "",
                    ["amount"] = $"{payment.Amount:N2} {payment.Currency}",
                    ["appointmentId"] = payment.Appointment.Id.ToString()
                });
            }

            // Single commit: the refund state and the notification row in one transaction.
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Refund retry succeeded for payment {PaymentId}.", paymentId);
            return;
        }

        if (attempt >= MaxAttempts)
        {
            _logger.LogError("Refund retries exhausted for payment {PaymentId} after {MaxAttempts} attempts.", paymentId, MaxAttempts);
            return;
        }

        var delay = TimeSpan.FromHours(Math.Pow(2, attempt));
        BackgroundJob.Schedule<RefundRetryJob>(job => job.RetryRefundAsync(paymentId, attempt + 1, CancellationToken.None), delay);
        _logger.LogWarning("Refund retry {Attempt} failed for payment {PaymentId}; rescheduled in {Delay} hours.", attempt, paymentId, delay.TotalHours);
    }

    private static async Task NotifySuperAdminsAsync(
        UserManager<ApplicationUser> userManager,
        IFcmService fcmService,
        Payment payment,
        string refundStatus)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["clinicName"] = "",
                ["amount"] = $"{payment.RefundAmount:N2} {payment.Currency}",
                ["refundStatus"] = refundStatus
            };

            var superAdmins = await userManager.GetUsersInRoleAsync(UserType.SuperAdmin.ToString());
            foreach (var admin in superAdmins.Where(a => !a.IsDeleted))
            {
                await fcmService.SendToUserAsync(admin.Id, NotificationType.SubscriptionCancelled, parameters);
            }
        }
        catch
        {
            // Notification failures must never break the retry flow.
        }
    }
}
