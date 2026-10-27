using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities;

/// <summary>
/// Append-only ledger of what the platform owes each clinic and what has been paid out.
/// The balance used for payouts is always derived from these rows, never re-computed
/// from payments, so a payout can never be duplicated.
/// </summary>
public class ClinicLedgerEntry : BaseEntity<Guid>, IClinicScopedEntity
{
    public Guid ClinicId { get; private set; }
    Guid? IClinicScopedEntity.ClinicId => ClinicId;

    public LedgerEntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public LedgerEntryStatus Status { get; private set; } = LedgerEntryStatus.Available;

    /// <summary>Entry becomes part of the payoutable balance at this moment (refund hold).</summary>
    public DateTime AvailableAt { get; private set; }

    public Guid? PaymentId { get; private set; }
    public Payment? Payment { get; private set; }
    public Guid? PayoutItemId { get; private set; }
    public string? Notes { get; private set; }

    private ClinicLedgerEntry() { }

    public static ClinicLedgerEntry CreateEarning(
        Guid clinicId,
        Guid paymentId,
        decimal netAmount,
        string currency,
        DateTime availableAt) => new()
    {
        ClinicId = clinicId,
        Type = LedgerEntryType.Earning,
        Amount = netAmount,
        Currency = string.IsNullOrWhiteSpace(currency) ? "EGP" : currency,
        AvailableAt = availableAt,
        PaymentId = paymentId
    };

    public static ClinicLedgerEntry CreateRefundDeduction(
        Guid clinicId,
        Guid paymentId,
        decimal netAmount,
        string currency) => new()
    {
        ClinicId = clinicId,
        Type = LedgerEntryType.RefundDeduction,
        Amount = -Math.Abs(netAmount),
        Currency = string.IsNullOrWhiteSpace(currency) ? "EGP" : currency,
        AvailableAt = DateTime.Now,
        PaymentId = paymentId
    };

    /// <summary>Money-out row written only once the transfer is confirmed by Paymob.</summary>
    public static ClinicLedgerEntry CreatePayout(
        Guid clinicId,
        Guid payoutItemId,
        decimal amount,
        string currency) => new()
    {
        ClinicId = clinicId,
        Type = LedgerEntryType.Payout,
        Amount = -Math.Abs(amount),
        Currency = string.IsNullOrWhiteSpace(currency) ? "EGP" : currency,
        Status = LedgerEntryStatus.Settled,
        AvailableAt = DateTime.Now,
        PayoutItemId = payoutItemId
    };

    public void Reserve(Guid payoutItemId)
    {
        Status = LedgerEntryStatus.Reserved;
        PayoutItemId = payoutItemId;
    }

    public void Settle()
    {
        Status = LedgerEntryStatus.Settled;
    }

    public void Release()
    {
        Status = LedgerEntryStatus.Available;
        PayoutItemId = null;
    }

    public void Cancel()
    {
        Status = LedgerEntryStatus.Cancelled;
    }

    /// <summary>
    /// Corrects the amount after the collected payment amount was reconciled. Only allowed
    /// while the entry is still part of the available balance — reserved/settled money
    /// must be corrected through an adjustment entry instead.
    /// </summary>
    public bool TryRevalue(decimal netAmount)
    {
        if (Status != LedgerEntryStatus.Available)
            return false;

        Amount = netAmount;
        return true;
    }

    public void AttachNote(string? note)
    {
        Notes = note;
    }
}
