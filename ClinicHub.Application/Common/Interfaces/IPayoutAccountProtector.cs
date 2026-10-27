namespace ClinicHub.Application.Common.Interfaces;

/// <summary>
/// Symmetric protection for bank account numbers so they are never stored in clear text.
/// </summary>
public interface IPayoutAccountProtector
{
    string Protect(string accountNumber);

    string Unprotect(string protectedAccountNumber);
}
