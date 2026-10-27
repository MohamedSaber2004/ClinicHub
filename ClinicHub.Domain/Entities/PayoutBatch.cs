using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities;

/// <summary>
/// One settlement run: the clinics it pays and the Paymob budget read around it.
/// Nothing leaves the platform until the batch is approved by someone other than its
/// creator (two-man rule), and it closes only when every item is resolved.
/// </summary>
public class PayoutBatch : BaseEntity<Guid>
{
    public string Code { get; private set; } = null!;
    public PayoutBatchStatus Status { get; private set; } = PayoutBatchStatus.Draft;

    public DateTime PeriodFrom { get; private set; }
    public DateTime PeriodTo { get; private set; }

    public decimal TotalAmount { get; private set; }
    public int ItemsCount { get; private set; }

    public decimal? BudgetBefore { get; private set; }
    public decimal? BudgetAfter { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? ExecutedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureNotes { get; private set; }

    private readonly List<PayoutItem> _items = new();
    public IReadOnlyCollection<PayoutItem> Items => _items;

    private PayoutBatch() { }

    public static PayoutBatch Create(
        string code,
        DateTime periodFrom,
        DateTime periodTo,
        Guid createdByUserId) => new()
    {
        Code = code,
        PeriodFrom = periodFrom,
        PeriodTo = periodTo,
        CreatedByUserId = createdByUserId
    };

    public bool IsOpen => Status is PayoutBatchStatus.Draft or PayoutBatchStatus.Approved;

    public PayoutItem AddItem(
        Guid clinicId,
        Guid payoutAccountId,
        decimal amount,
        string currency,
        Guid itemId)
    {
        var item = new PayoutItem(Id, clinicId, payoutAccountId, amount, currency, itemId);
        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    public bool Approve(Guid approverUserId, bool requireTwoManApproval)
    {
        if (!IsOpen || Status == PayoutBatchStatus.Approved)
            return false;
        if (ItemsCount == 0)
            return false;
        if (requireTwoManApproval && approverUserId == CreatedByUserId)
            return false;

        ApprovedByUserId = approverUserId;
        ApprovedAt = AppNow;
        Status = PayoutBatchStatus.Approved;
        return true;
    }

    /// <summary>A retried item re-opens an already closed run so the batch stays truthful.</summary>
    public bool ReopenForRetry()
    {
        if (Status is not (PayoutBatchStatus.PartiallyFailed or PayoutBatchStatus.Failed))
            return false;
        if (_items.All(i => i.IsResolved))
            return false;

        Status = PayoutBatchStatus.Processing;
        return true;
    }

    public bool StartExecution(decimal? budgetBefore)
    {
        if (Status != PayoutBatchStatus.Approved)
            return false;

        Status = PayoutBatchStatus.Processing;
        BudgetBefore = budgetBefore;
        ExecutedAt = AppNow;
        return true;
    }

    /// <summary>Stays open while any item is unresolved, so a partial run can be resumed.</summary>
    public bool Complete(decimal? budgetAfter)
    {
        if (Status != PayoutBatchStatus.Processing)
            return false;
        if (_items.Any(i => !i.IsResolved))
            return false;

        Status = _items.All(i => i.Status == PayoutItemStatus.Success)
            ? PayoutBatchStatus.Completed
            : _items.Any(i => i.Status == PayoutItemStatus.Success)
                ? PayoutBatchStatus.PartiallyFailed
                : PayoutBatchStatus.Failed;

        BudgetAfter = budgetAfter;
        CompletedAt = AppNow;
        RecalculateTotals();
        return true;
    }

    public bool Cancel()
    {
        if (!IsOpen)
            return false;

        Status = PayoutBatchStatus.Cancelled;
        return true;
    }

    public void NoteFailure(string? notes) => FailureNotes = notes;

    private void RecalculateTotals()
    {
        var payable = _items.Where(i => i.Status != PayoutItemStatus.Cancelled).ToList();
        ItemsCount = payable.Count;
        TotalAmount = payable.Sum(i => i.Amount);
    }
}
