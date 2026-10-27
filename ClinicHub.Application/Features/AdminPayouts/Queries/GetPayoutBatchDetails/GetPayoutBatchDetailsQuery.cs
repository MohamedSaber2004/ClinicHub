using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBatchDetails;

public class GetPayoutBatchDetailsQuery : IRequest<PayoutBatchDto>
{
    public Guid BatchId { get; set; }
}
