using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ExecutePayoutBatch;

/// <summary>Queues an approved batch for disbursement. The money moves in the background job.</summary>
public class ExecutePayoutBatchCommand : IRequest<PayoutBatchDto>
{
    public Guid BatchId { get; set; }
}
