using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.DisbursePayoutItem;

/// <summary>What one disbursement attempt ended as, from our point of view.</summary>
public enum PayoutDisbursementOutcome
{
    /// <summary>Paymob accepted the transfer; settlement waits for the inquiry.</summary>
    Sent = 0,

    /// <summary>Paymob refused it outright and the reservation was handed back.</summary>
    Rejected = 1,

    /// <summary>Answer lost or still pending: the item must be inquired, not re-sent.</summary>
    AwaitingConfirmation = 2,

    /// <summary>Nothing to do — already resolved or the batch is not executing.</summary>
    Skipped = 3
}

/// <summary>
/// The only place a payout item talks to Paymob. Safe to call repeatedly for the same item:
/// anything already sent or already answered is proven by inquiry before a new transfer is made.
/// </summary>
public class DisbursePayoutItemCommand : IRequest<PayoutDisbursementOutcome>
{
    public Guid ItemId { get; set; }
}
