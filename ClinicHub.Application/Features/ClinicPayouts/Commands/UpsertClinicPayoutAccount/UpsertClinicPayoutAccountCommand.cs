using ClinicHub.Application.Features.ClinicPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.ClinicPayouts.Commands.UpsertClinicPayoutAccount;

public class UpsertClinicPayoutAccountCommand : IRequest<ClinicPayoutAccountDto>
{
    public string HolderName { get; set; } = string.Empty;

    /// <summary>Required by Paymob on every disbursement; stored encrypted, never returned raw.</summary>
    public string NationalId { get; set; } = string.Empty;

    public string BankCode { get; set; } = string.Empty;

    /// <summary>Bank account number, IBAN or card number, exactly as the bank issued it.</summary>
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankTransactionType { get; set; }
}
