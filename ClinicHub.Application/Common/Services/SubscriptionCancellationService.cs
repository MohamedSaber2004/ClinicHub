using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Application.Features.Subscriptions.Services;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Common.Services;

/// <summary>
/// Shared cancellation engine. Cancellation is committed in one DB transaction
/// (subscription state + refund intent); the Paymob refund is sent only after
/// that commit is durable. An ambiguous gateway outcome becomes NeedsReview and
/// is never auto-retried, so a refund can never be issued twice.
/// </summary>
public class SubscriptionCancellationService : ISubscriptionCancellationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobService _paymobService;
    private readonly IBackgroundJobScheduler _jobScheduler;
    private readonly IFcmService _fcmService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SubscriptionCancellationService> _logger;

    public SubscriptionCancellationService(
        IUnitOfWork unitOfWork,
        IPaymobService paymobService,
        IBackgroundJobScheduler jobScheduler,
        IFcmService fcmService,
        UserManager<ApplicationUser> userManager,
        ILogger<SubscriptionCancellationService> logger)
    {
        _unitOfWork = unitOfWork;
        _paymobService = paymobService;
        _jobScheduler = jobScheduler;
        _fcmService = fcmService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<CancelSubscriptionResponseDto> CancelAsync(
        Guid subscriptionId,
        Guid? clinicIdScope,
        SubscriptionCancellationSource source,
        Guid? cancelledByUserId,
        decimal? refundAmountOverride,
        string? reason,
        bool requireOnlinePayment,
        CancellationToken cancellationToken)
    {
        var subscription = await _unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllWithIncluding(
                s => s.Id == subscriptionId
                     && (clinicIdScope == null || s.ClinicId == clinicIdScope),
                s => s.Payment,
                s => s.Plan)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
            throw new NotFoundException(LocalizationKeys.SubscriptionMessages.NotFound.Value);

        if (subscription.Status != SubscriptionStatus.Active)
            throw new BadRequestException(LocalizationKeys.SubscriptionMessages.AlreadyCancelled.Value);

        if (subscription.EndDate <= DateTime.Now)
            throw new BadRequestException(LocalizationKeys.SubscriptionMessages.ExpiredCannotCancel.Value);

        var payment = subscription.Payment;
        if (payment == null)
            throw new BadRequestException(LocalizationKeys.SubscriptionMessages.NoPaymentLinked.Value);

        if (payment.Status is not (PaymentStatus.Paid or PaymentStatus.PartiallyRefunded))
            throw new BadRequestException(LocalizationKeys.SubscriptionMessages.NoPaymentLinked.Value);

        var isOnlinePayment = long.TryParse(payment.PaymobTransactionId, out _);

        if (requireOnlinePayment && !isOnlinePayment)
            throw new BadRequestException(LocalizationKeys.SubscriptionMessages.OnlyOnlineCanCancel.Value);

        if (payment.RefundedAmount >= payment.Amount)
            throw new BadRequestException(LocalizationKeys.PaymentMessages.AlreadyRefunded.Value);

        var now = DateTime.Now;
        var quote = SubscriptionRefundCalculator.Calculate(
            payment.Amount,
            payment.RefundedAmount,
            subscription.StartDate,
            subscription.EndDate,
            now);

        var remainingPaid = Math.Max(0m, payment.Amount - payment.RefundedAmount);
        decimal refundAmount;

        if (refundAmountOverride.HasValue)
        {
            if (refundAmountOverride.Value < 0)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            if (refundAmountOverride.Value > remainingPaid)
                throw new BadRequestException(LocalizationKeys.SubscriptionMessages.RefundExceedsPaid.Value);

            if (refundAmountOverride.Value > 0 && !isOnlinePayment)
                throw new BadRequestException(LocalizationKeys.SubscriptionMessages.OnlyOnlineCanCancel.Value);

            refundAmount = Math.Round(refundAmountOverride.Value, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            refundAmount = quote.RefundAmount;
        }

        // --- Commit cancellation + refund intent atomically ---
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            subscription.Status = source == SubscriptionCancellationSource.Admin
                ? SubscriptionStatus.Revoked
                : SubscriptionStatus.Cancelled;
            subscription.CancelledAt = now;
            subscription.CancellationSource = source;
            subscription.CancelledByUserId = cancelledByUserId;
            subscription.CancellationReason = reason;
            subscription.RefundAmount = refundAmount;

            if (!isOnlinePayment && refundAmount == 0)
            {
                var note = "Cancelled by admin; refund handled outside Paymob.";
                subscription.Notes = string.IsNullOrWhiteSpace(subscription.Notes)
                    ? note
                    : $"{subscription.Notes} | {note}";
            }

            _unitOfWork.GetRepository<Subscription, Guid>().Update(subscription);

            if (refundAmount > 0)
            {
                payment.RequestRefund(refundAmount, "Pro-rated subscription cancellation");
                _unitOfWork.PaymentRepository.Update(payment);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        // --- Send the refund after the commit is durable ---
        if (refundAmount > 0)
        {
            await RefundAsync(payment.Id, cancellationToken);
        }

        await NotifyCancellationAsync(subscription, payment, cancellationToken);

        return new CancelSubscriptionResponseDto
        {
            SubscriptionId = subscription.Id,
            CancelledAt = now,
            TotalDays = quote.TotalDays,
            UsedDays = quote.UsedDays,
            ChargedAmount = quote.ChargeAmount,
            RefundAmount = refundAmount,
            Currency = payment.Currency,
            RefundStatus = payment.RefundStatus,
            RefundId = payment.RefundId
        };
    }

    public async Task RefundAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        await PaymentRefundGate.RunAsync(paymentId, async () =>
        {
            // Re-read inside the gate: another request may already own this refund.
            var payment = await _unitOfWork.PaymentRepository.GetAllAsync(p => p.Id == paymentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (payment == null)
                return;

            if (payment.RefundStatus != PaymentRefundStatus.Pending)
                return; // another request owns it (Processing/Completed/Failed/NeedsReview)

            if (!long.TryParse(payment.PaymobTransactionId, out _))
            {
                // Definite failure — not an online payment.
                payment.FailRefund("Missing or invalid Paymob transaction id.");
                _unitOfWork.PaymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            // Persisted claim; Payment.Version makes a concurrent claim fail with 409.
            if (!payment.ClaimRefund())
                return;

            _unitOfWork.PaymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                var refund = await _paymobService.RefundTransactionAsync(
                    payment.PaymobTransactionId!, payment.RefundAmount, cancellationToken);

                if (refund.Success)
                {
                    payment.CompleteRefund(payment.RefundAmount, refund.RefundId ?? refund.Message);
                }
                else
                {
                    // Definite gateway failure → retryable.
                    payment.FailRefund(refund.Message ?? "Refund failed");
                }

                _unitOfWork.PaymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                if (!refund.Success)
                {
                    await _jobScheduler.ScheduleRefundRetryAsync(payment.Id);
                    await NotifySuperAdminsAsync(payment, NotificationType.RefundProcessed,
                        "refundFailed", cancellationToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Ambiguous outcome (timeout/crash). Never auto-retried.
                _logger.LogError(ex,
                    "Ambiguous Paymob refund outcome for payment {PaymentId}; marked NeedsReview.", paymentId);

                payment.RequireRefundReview(ex.Message);
                _unitOfWork.PaymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                await NotifySuperAdminsAsync(payment, NotificationType.RefundProcessed,
                    "needsReview", cancellationToken);
            }
        });
    }

    private async Task NotifyCancellationAsync(Subscription subscription, Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(subscription.ClinicId);
            if (clinic == null) return;

            var parameters = new Dictionary<string, object>
            {
                ["clinicName"] = clinic.Name ?? "",
                ["amount"] = $"{subscription.RefundAmount:N2} {payment.Currency}",
                ["refundStatus"] = payment.RefundStatus.ToString()
            };

            // Owner
            if (clinic.ClinicAdminId.HasValue)
            {
                await _fcmService.SendToUserAsync(clinic.ClinicAdminId.Value,
                    NotificationType.SubscriptionCancelled, parameters);
            }

            // Superadmins: visibility on owner-initiated cancellations, plus all refund problems.
            var notifyAdmins = subscription.CancellationSource != SubscriptionCancellationSource.Admin
                               || payment.RefundStatus is PaymentRefundStatus.Failed
                                   or PaymentRefundStatus.NeedsReview;

            if (notifyAdmins)
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync(UserType.SuperAdmin.ToString());
                foreach (var admin in superAdmins.Where(a => !a.IsDeleted))
                {
                    await _fcmService.SendToUserAsync(admin.Id,
                        NotificationType.SubscriptionCancelled, parameters);
                }
            }
        }
        catch (Exception ex)
        {
            // Notifications must never break the financial flow.
            _logger.LogError(ex, "Cancellation notifications failed for subscription {SubscriptionId}.",
                subscription.Id);
        }
    }

    private async Task NotifySuperAdminsAsync(Payment payment, NotificationType type, string status, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(payment.ClinicId);
            var parameters = new Dictionary<string, object>
            {
                ["clinicName"] = clinic?.Name ?? "",
                ["amount"] = $"{payment.RefundAmount:N2} {payment.Currency}",
                ["refundStatus"] = status
            };

            var superAdmins = await _userManager.GetUsersInRoleAsync(UserType.SuperAdmin.ToString());
            foreach (var admin in superAdmins.Where(a => !a.IsDeleted))
            {
                await _fcmService.SendToUserAsync(admin.Id, type, parameters);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Superadmin refund notification failed for payment {PaymentId}.", payment.Id);
        }
    }
}
