using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.CreatePayoutBatch;

/// <summary>
/// Freezes the currently eligible balances into a draft batch and reserves their ledger rows.
/// Nothing is sent to Paymob here.
/// </summary>
public class CreatePayoutBatchCommand : IRequest<PayoutBatchDto>
{
    /// <summary>Overrides the configured floor for this run only; never below Paymob's minimum.</summary>
    public decimal? MinimumAmount { get; set; }

    /// <summary>Optional subset of clinics to pay; the preview's eligibility rules still apply.</summary>
    public List<Guid>? ClinicIds { get; set; }
}
