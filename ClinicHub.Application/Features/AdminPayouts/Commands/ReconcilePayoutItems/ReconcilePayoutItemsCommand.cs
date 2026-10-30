using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ReconcilePayoutItems;

/// <summary>
/// Asks Paymob for the real status of every transfer still marked as sent and settles or
/// releases the ledger accordingly. Also the safety net when a callback never arrives.
/// </summary>
public class ReconcilePayoutItemsCommand : IRequest<int>
{
    public Guid? BatchId { get; set; }
}
