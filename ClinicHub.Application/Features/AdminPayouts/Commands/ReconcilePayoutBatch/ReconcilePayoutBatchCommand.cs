using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ReconcilePayoutBatch;

/// <summary>
/// Asks Paymob for the current status of a run's unresolved transfers without waiting for
/// the scheduled sweep. Safe to repeat: it only reads Paymob and closes a finished run.
/// </summary>
public class ReconcilePayoutBatchCommand : IRequest<PayoutBatchDto>
{
    public Guid BatchId { get; set; }
}
