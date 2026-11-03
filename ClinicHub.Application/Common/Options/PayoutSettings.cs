namespace ClinicHub.Application.Common.Options;

public class PayoutSettings
{
    public string BaseUrl { get; set; } = "https://stagingpayouts.paymobsolutions.com";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string CallbackSecret { get; set; } = string.Empty;

    public string DefaultBankTransactionType { get; set; } = "cash_transfer";

    /// <summary>Smallest amount worth a payout cycle (keeps the batch economical).</summary>
    public decimal MinimumPayoutAmount { get; set; } = 120m;

    /// <summary>Paymob's minimum accepted disbursement amount.</summary>
    public decimal MinimumTransactionAmount { get; set; } = 112m;

    /// <summary>How long an earning stays out of the payoutable balance to cover refunds.</summary>
    public int RefundHoldDays { get; set; } = 7;

    public bool RequireTwoManApproval { get; set; } = true;
    public bool AutoExecute { get; set; }
    public int MaxRetriesPerItem { get; set; } = 2;
}
