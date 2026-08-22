using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Common.Interfaces;

public interface ISubscriptionPaymentCompleter
{
    /// <summary>
    /// Idempotently activates a subscription from a paid subscription-type payment:
    /// revokes existing active subscriptions of the clinic, creates the new active
    /// subscription and links it to the payment. Does NOT save changes — the caller
    /// owns the unit of work.
    /// </summary>
    Task<Guid> ActivateFromPaymentAsync(Payment payment, CancellationToken cancellationToken);
}
