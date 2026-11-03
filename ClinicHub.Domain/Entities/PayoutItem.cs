using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities;

/// <summary>
/// One clinic inside a payout batch, and the Paymob disbursement it became.
/// <see cref="PaymobTransactionId"/> is unique per item and every attempt bumps
/// <see cref="Attempts"/>, so a timeout can be retried only after inquiry proves the
/// first attempt never landed.
/// </summary>
public class PayoutItem : BaseEntity<Guid>, IClinicScopedEntity
{
    public Guid BatchId { get; private set; }
    public PayoutBatch Batch { get; private set; } = null!;

    public Guid ClinicId { get; private set; }
    Guid? IClinicScopedEntity.ClinicId => ClinicId;
    public Clinic Clinic { get; private set; } = null!;

    public Guid PayoutAccountId { get; private set; }
    public ClinicPayoutAccount PayoutAccount { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public PayoutItemStatus Status { get; private set; } = PayoutItemStatus.Pending;

    public string? PaymobTransactionId { get; private set; }
    public string? PaymobStatusCode { get; private set; }
    public string? DisbursementStatusRaw { get; private set; }
    public string? LastError { get; private set; }

    public int Attempts { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? SettledAt { get; private set; }

    private PayoutItem() { }

    public PayoutItem(
        Guid batchId,
        Guid clinicId,
        Guid payoutAccountId,
        decimal amount,
        string currency,
        Guid? id = null)
    {
        // Ledger rows are reserved by item id, so the id has to exist before the item does.
        if (id.HasValue)
            Id = id.Value;

        BatchId = batchId;
        ClinicId = clinicId;
        PayoutAccountId = payoutAccountId;
        Amount = amount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "EGP" : currency;
    }

    public bool IsResolved => Status is PayoutItemStatus.Success
        or PayoutItemStatus.Failed
        or PayoutItemStatus.Cancelled;

    /// <summary>
    /// Paymob may already hold the money even when our call failed, so anything with a
    /// transaction id must be inquired instead of re-sent.
    /// </summary>
    public bool RequiresInquiryBeforeRetry => Status == PayoutItemStatus.Processing || PaymobTransactionId is not null;

    /// <summary>
    /// A retry is only ever blind-safe when Paymob holds no transaction for this item;
    /// the disbursement handler still proves that by inquiry before sending again.
    /// </summary>
    public bool CanRetry(int maxAttempts) =>
        PaymobTransactionId is null
        && Attempts < maxAttempts
        && (Status == PayoutItemStatus.Failed
            || (Status == PayoutItemStatus.Pending && Attempts > 0));

    /// <summary>Puts a failed item back in the queue; the money it was reserved for stays reserved.</summary>
    public bool QueueForRetry(int maxAttempts)
    {
        if (!CanRetry(maxAttempts))
            return false;

        Status = PayoutItemStatus.Pending;
        LastError = null;
        return true;
    }

    public void MarkSent(string transactionId, string? disbursementStatus, string? statusCode)
    {
        PaymobTransactionId = transactionId;
        DisbursementStatusRaw = disbursementStatus;
        PaymobStatusCode = statusCode;
        Status = PayoutItemStatus.Processing;
        SentAt = AppNow;
        NoteAttempt();
    }

    /// <summary>Records a rejected or ambiguous attempt without giving up on the item.</summary>
    public void NoteAttempt(string? statusCode = null, string? disbursementStatus = null, string? error = null)
    {
        Attempts++;
        LastAttemptAt = AppNow;
        if (statusCode is not null) PaymobStatusCode = statusCode;
        if (disbursementStatus is not null) DisbursementStatusRaw = disbursementStatus;
        if (error is not null) LastError = error;
    }

    public void MarkSuccess(string? statusCode, string? disbursementStatus)
    {
        Status = PayoutItemStatus.Success;
        PaymobStatusCode = statusCode ?? PaymobStatusCode;
        DisbursementStatusRaw = disbursementStatus ?? DisbursementStatusRaw;
        SettledAt = AppNow;
        LastError = null;
    }

    public void MarkFailed(string? statusCode, string? disbursementStatus, string? error)
    {
        Status = PayoutItemStatus.Failed;
        PaymobStatusCode = statusCode ?? PaymobStatusCode;
        DisbursementStatusRaw = disbursementStatus ?? DisbursementStatusRaw;
        LastError = error;
        SettledAt = null;
    }

    public bool Cancel()
    {
        if (Status != PayoutItemStatus.Pending)
            return false;

        Status = PayoutItemStatus.Cancelled;
        return true;
    }
}
