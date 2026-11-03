using ClinicHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace ClinicHub.Infrastructure.Services.Payouts;

public class DataProtectionPayoutAccountProtector : IPayoutAccountProtector
{
    private static readonly string Purpose = "ClinicHub.PayoutAccounts.v1";

    private readonly IDataProtector _protector;

    public DataProtectionPayoutAccountProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose);
    }

    public string Protect(string accountNumber) => _protector.Protect(accountNumber);

    public string Unprotect(string protectedAccountNumber) => _protector.Unprotect(protectedAccountNumber);
}
