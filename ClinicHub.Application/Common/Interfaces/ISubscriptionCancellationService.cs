using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Common.Interfaces
{
    /// <summary>
    /// Shared engine for cancelling a subscription with a pro-rated refund.
    /// Used by the owner self-cancel endpoint, the admin revoke endpoint,
    /// and (via RefundAsync) the background refund retry job.
    /// </summary>
    public interface ISubscriptionCancellationService
    {
        /// <summary>
        /// Cancels the subscription, freezes the refund amount on the payment, and —
        /// after the DB commit — sends the refund to Paymob. The cancellation is
        /// committed first; an ambiguous Paymob outcome never rolls it back.
        /// </summary>
        Task<CancelSubscriptionResponseDto> CancelAsync(
            Guid subscriptionId,
            Guid? clinicIdScope,
            SubscriptionCancellationSource source,
            Guid? cancelledByUserId,
            decimal? refundAmountOverride,
            string? reason,
            bool requireOnlinePayment,
            CancellationToken cancellationToken);

        /// <summary>
        /// Claims and processes the frozen refund for a payment (Paymob call).
        /// Safe to call multiple times — only a Pending refund is processed.
        /// </summary>
        Task RefundAsync(Guid paymentId, CancellationToken cancellationToken);
    }
}
