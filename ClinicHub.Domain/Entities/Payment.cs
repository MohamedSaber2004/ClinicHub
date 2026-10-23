using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities;

public class Payment : BaseEntity<Guid>, IClinicScopedEntity
{
    public PaymentType Type { get; private set; } = PaymentType.Appointment;
    public string? Code { get; private set; }
    public string? RefNumber { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public Appointment? Appointment { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ClinicId { get; private set; }
    Guid? IClinicScopedEntity.ClinicId => ClinicId;
    public Clinic Clinic { get; private set; } = null!;
    public Guid? SubscriptionId { get; private set; }
    public Subscription? Subscription { get; private set; }
    public Guid? PlanId { get; set; }
    public SubscriptionPlan? SubscriptionPeriod { get; set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public string? PaymobOrderId { get; set; }
    public string? PaymobTransactionId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public string? RedirectUrl { get; private set; }
    public string? FailureReason { get; private set; }
    public string? TransactionId { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public string? RefundReason { get; private set; }
    public PaymentRefundStatus RefundStatus { get; private set; } = PaymentRefundStatus.None;
    public decimal RefundAmount { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public string? RefundId { get; private set; }
    public DateTime? RefundRequestedAt { get; private set; }
    public int RefundAttemptCount { get; private set; }
    public string? RefundFailureReason { get; private set; }
    public string? Notes { get; private set; }

    private Payment() { }

    public Payment(PaymentType type, Guid userId, Guid clinicId, decimal amount, string currency = "EGP")
    {
        Type = type;
        UserId = userId;
        ClinicId = clinicId;
        Amount = amount;
        Currency = currency ?? "EGP";
        Code = "#P-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
        RefNumber = $"PM-{DateTime.Now.Year}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }

    public void LinkToSubscription(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public void LinkToAppointment(Guid appointmentId)
    {
        AppointmentId = appointmentId;
    }

    public void SetManualReference(string? refNumber, string? notes)
    {
        RefNumber = string.IsNullOrWhiteSpace(refNumber) ? RefNumber : refNumber;
        Notes = notes;
    }

        public void MarkAsProcessing(string? redirectUrl = null, string? paymentMethod = null)
        {
            RedirectUrl = redirectUrl;
            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Processing;
        }

        public void SetPaymobCheckout(string redirectUrl)
        {
            RedirectUrl = redirectUrl;
            Status = PaymentStatus.Pending;
            FailureReason = null;
        }

    public void MarkAsPaid(string transactionId, string method)
    {
        PaymobTransactionId = transactionId;
        TransactionId = transactionId;
        PaymentMethod = method;
        PaidAt = DateTime.Now;
        Status = PaymentStatus.Paid;
        FailureReason = null;
    }

    public void MarkAsManuallyPaid(string method, string? reference = null)
    {
        TransactionId = reference;
        PaymentMethod = method;
        PaidAt = DateTime.Now;
        Status = PaymentStatus.Paid;
        FailureReason = null;
    }

    public void MarkAsFailed(string? failureReason = null)
    {
        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
    }

    public void MarkAsRefunded(string? reason = null)
    {
        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.Now;
        RefundReason = reason;
        PaidAt = null;
        PaymentMethod = null;
    }

    /// <summary>
    /// Freezes the amount that will be sent to the payment gateway. Persisted before
    /// the external call so a retry always refunds the exact same amount.
    /// </summary>
    public void RequestRefund(decimal amount, string? reason = null)
    {
        RefundStatus = PaymentRefundStatus.Pending;
        RefundAmount = amount;
        RefundAttemptCount = 0;
        RefundFailureReason = null;
        RefundRequestedAt = DateTime.Now;
        RefundReason = reason;
    }

    /// <summary>
    /// Atomically takes ownership of a pending refund. Returns false when another
    /// request already moved it out of Pending.
    /// </summary>
    public bool ClaimRefund()
    {
        if (RefundStatus != PaymentRefundStatus.Pending)
            return false;

        RefundStatus = PaymentRefundStatus.Processing;
        RefundAttemptCount++;
        RefundRequestedAt = DateTime.Now;
        return true;
    }

    public void CompleteRefund(decimal amount, string? refundId)
    {
        RefundedAmount += amount;
        RefundId = refundId;
        RefundStatus = PaymentRefundStatus.Completed;
        RefundedAt = DateTime.Now;
        RefundFailureReason = null;
        // PaidAt and PaymentMethod are intentionally preserved for audit.
        Status = RefundedAmount >= Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;
    }

    /// <summary>
    /// A definite gateway failure. The refund returns to Pending so it can be retried
    /// with the same frozen amount.
    /// </summary>
    public void FailRefund(string? reason)
    {
        RefundStatus = PaymentRefundStatus.Pending;
        RefundFailureReason = reason;
    }

    /// <summary>
    /// The gateway outcome is unknown (timeout / crash). Never retried automatically,
    /// because the refund may already have been processed.
    /// </summary>
    public void RequireRefundReview(string? reason)
    {
        RefundStatus = PaymentRefundStatus.NeedsReview;
        RefundFailureReason = reason;
    }

    /// <summary>
    /// Retry attempts exhausted. The refund is abandoned until an admin intervenes;
    /// it is no longer eligible for automatic retries.
    /// </summary>
    public void AbandonRefund(string? reason)
    {
        RefundStatus = PaymentRefundStatus.Failed;
        RefundFailureReason = reason;
    }
}