using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.ClinicPayouts.DTOs;

public class ClinicPayoutSummaryDto
{
    /// <summary>Earnings past the refund hold, not yet reserved by a batch.</summary>
    public decimal AvailableBalance { get; set; }

    /// <summary>Earnings still inside the refund hold window.</summary>
    public decimal PendingBalance { get; set; }

    /// <summary>Earnings reserved by an open payout batch.</summary>
    public decimal ReservedBalance { get; set; }

    /// <summary>Total already transferred to the clinic's bank account.</summary>
    public decimal PaidTotal { get; set; }

    public string Currency { get; set; } = "EGP";
    public decimal MinimumPayoutAmount { get; set; }
    public PayoutAccountStatus? AccountStatus { get; set; }
    public bool IsPayoutEnabled { get; set; }
}
