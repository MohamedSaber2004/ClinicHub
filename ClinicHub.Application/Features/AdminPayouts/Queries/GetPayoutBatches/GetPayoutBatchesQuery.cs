using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBatches;

public class GetPayoutBatchesQuery : IRequest<PagginatedResult<PayoutBatchDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public PayoutBatchStatus? Status { get; set; }
}
