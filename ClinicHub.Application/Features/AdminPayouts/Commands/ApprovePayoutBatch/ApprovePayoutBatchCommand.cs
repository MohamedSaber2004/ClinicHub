using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ApprovePayoutBatch;

public class ApprovePayoutBatchCommand : IRequest<PayoutBatchDto>
{
    public Guid BatchId { get; set; }
}
