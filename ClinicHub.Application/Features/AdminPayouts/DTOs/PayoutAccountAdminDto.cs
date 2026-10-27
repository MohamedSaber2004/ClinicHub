using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayouts.DTOs;

public class PayoutAccountAdminDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string MaskedAccountNumber { get; set; } = string.Empty;
    public string MaskedNationalId { get; set; } = string.Empty;
    public string BankTransactionType { get; set; } = string.Empty;
    public PayoutAccountStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }

    public static PayoutAccountAdminDto From(ClinicPayoutAccount account, string clinicName) => new()
    {
        Id = account.Id,
        ClinicId = account.ClinicId,
        ClinicName = clinicName,
        HolderName = account.HolderName,
        BankCode = account.BankCode,
        MaskedAccountNumber = $"**** {account.AccountLast4}",
        BankTransactionType = account.BankTransactionType,
        Status = account.Status,
        SubmittedAt = account.CreatedAt,
        VerifiedAt = account.VerifiedAt,
        RejectionReason = account.RejectionReason
    };
}
