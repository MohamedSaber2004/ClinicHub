using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.ClinicPayouts.DTOs;

public class ClinicPayoutAccountDto
{
    public Guid Id { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string MaskedAccountNumber { get; set; } = string.Empty;
    public string MaskedNationalId { get; set; } = string.Empty;
    public string BankTransactionType { get; set; } = string.Empty;
    public PayoutAccountStatus Status { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }
}
