using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.UpsertPayoutAccountAsAdmin;

/// <summary>
/// Lets a SuperAdmin enter a clinic's bank details directly. Saving from the admin side also
/// verifies the account, because a destination nobody in the platform reviewed must never
/// receive money.
/// </summary>
public class UpsertPayoutAccountAsAdminCommand : IRequest<PayoutAccountAdminDto>
{
    public Guid ClinicId { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? BankTransactionType { get; set; }
}
