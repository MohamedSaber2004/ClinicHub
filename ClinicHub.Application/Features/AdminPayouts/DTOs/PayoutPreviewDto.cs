using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayouts.DTOs;

public class PayoutPreviewDto
{
    public decimal MinimumAmount { get; set; }
    public decimal TotalEligible { get; set; }
    public List<PayoutPreviewItemDto> Items { get; set; } = new();
}

public class PayoutPreviewItemDto
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public string Currency { get; set; } = "EGP";
    public bool Eligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public PayoutAccountStatus? AccountStatus { get; set; }
    public string? AccountLast4 { get; set; }
}
