using ClinicHub.Application.Common.Models;

namespace ClinicHub.Application.Common.Interfaces;

/// <summary>
/// Paymob Payouts (disbursement) client. It is a different Paymob product from the Accept
/// collections API behind <see cref="IPaymobService"/>: different base URL, different
/// credentials, and money leaves the platform through it.
/// </summary>
public interface IPayoutService
{
    /// <summary>The prepaid float disbursements are debited from. Every batch is checked against it.</summary>
    Task<PayoutBudgetResult> GetBudgetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends one bank disbursement. <paramref name="request"/>.<see cref="PayoutBankDisbursementRequest.ClientReferenceId"/>
    /// must be stable per payout item: when the answer is <see cref="PayoutOutcome.Unknown"/>,
    /// that reference is the only way to prove whether Paymob took the money or not.
    /// </summary>
    Task<PayoutDisbursementResult> DisburseBankAsync(
        PayoutBankDisbursementRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PayoutTransactionStatus>> InquireAsync(
        IReadOnlyCollection<string> transactionIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PayoutTransactionStatus>> InquireByReferenceAsync(
        IReadOnlyCollection<string> clientReferenceIds,
        CancellationToken cancellationToken = default);
}
