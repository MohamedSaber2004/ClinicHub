namespace ClinicHub.Domain.Common;

/// <summary>
/// Paymob Payout bank codes (closed list from the disbursement API contract).
/// Stored verbatim so the payout request needs no translation layer.
/// </summary>
public static class PayoutBankCodes
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AUB", "MIDB", "BDC", "HSBC", "CAE", "EGB", "UB", "QNB", "ARAB", "ENBD",
        "ABK", "NBK", "ABC", "FAB", "ADIB", "CIB", "HDB", "MISR", "AAIB", "EALB",
        "EDBE", "FAIB", "BLOM", "ADCB", "BOA", "SAIB", "NBE", "ABRK", "POST", "NSB",
        "IDB", "SCB", "MASH", "AIB", "GASC", "ARIB", "PDAC", "NBG", "CBE", "BBE"
    };

    public static bool IsValid(string? code) =>
        !string.IsNullOrWhiteSpace(code) && All.Contains(code.Trim());

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();
}
