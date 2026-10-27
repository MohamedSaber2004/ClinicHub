using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutHistory;

/// <summary>Every transfer made to the clinic, newest first — its payout statement.</summary>
public class GetClinicPayoutHistoryQuery : IRequest<PagginatedResult<PayoutItemDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
