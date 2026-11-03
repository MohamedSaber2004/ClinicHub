namespace ClinicHub.Domain.Common;

/// <summary>
/// Paymob Payout bank transaction types. Values match the API contract exactly.
/// </summary>
public static class PayoutBankTransactionTypes
{
    public const string CashTransfer = "cash_transfer";
    public const string Salary = "salary";
    public const string CreditCard = "credit_card";
    public const string PrepaidCard = "prepaid_card";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        CashTransfer, Salary, CreditCard, PrepaidCard
    };

    public static bool IsValid(string? type) =>
        !string.IsNullOrWhiteSpace(type) && All.Contains(type.Trim());

    public static string Normalize(string? type) =>
        string.IsNullOrWhiteSpace(type) ? CashTransfer : type.Trim().ToLowerInvariant();
}
