using ClinicHub.Application.Common.Models;

namespace ClinicHub.Application.Common.Interfaces;

/// <summary>
/// Turns a Paymob transaction status into ledger truth. Settlement happens only here, and only
/// from an inquiry — callbacks alone never move money in our books.
/// </summary>
public interface IPayoutStatusApplier
{
    /// <summary>Returns true when the item (and therefore the ledger) changed.</summary>
    Task<bool> ApplyAsync(PayoutTransactionStatus status, CancellationToken cancellationToken = default);

    Task<bool> ApplyByTransactionIdAsync(string paymobTransactionId, CancellationToken cancellationToken = default);

    Task<bool> CloseBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
}
