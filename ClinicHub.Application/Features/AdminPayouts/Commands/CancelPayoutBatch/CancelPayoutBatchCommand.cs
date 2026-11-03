using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.CancelPayoutBatch;

/// <summary>Abandons a run before it executes and hands every reserved earning back.</summary>
public class CancelPayoutBatchCommand : IRequest<PayoutBatchDto>
{
    public Guid BatchId { get; set; }
}
