using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.RetryPayoutItem;

/// <summary>
/// Re-queues one failed clinic inside a run. Only items Paymob never received are retried —
/// an item that already carries a transaction id must be inquired first.
/// </summary>
public class RetryPayoutItemCommand : IRequest<PayoutBatchDto>
{
    public Guid ItemId { get; set; }
}
