using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Common.Interfaces;

public interface IClinicLedgerService
{
    /// <summary>
    /// Captures the revenue split on the payment and appends the clinic's earning to the
    /// ledger exactly once per payment.
    /// </summary>
    Task AccrueAppointmentEarningAsync(
        Payment payment,
        decimal platformFeePercent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Holds a clinic's matured earnings for one payout item and returns what was reserved.
    /// Reserving is what stops the same earning from being paid in two batches.
    /// </summary>
    Task<decimal> ReserveForPayoutItemAsync(
        Guid clinicId,
        Guid payoutItemId,
        CancellationToken cancellationToken = default);

    /// <summary>Marks reserved earnings settled and appends the matching payout row.</summary>
    Task SettlePayoutItemAsync(PayoutItem item, CancellationToken cancellationToken = default);

    /// <summary>Hands every earning held by an item back to the available balance.</summary>
    Task ReleasePayoutItemAsync(Guid payoutItemId, CancellationToken cancellationToken = default);
}
