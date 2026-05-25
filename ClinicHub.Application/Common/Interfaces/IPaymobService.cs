using ClinicHub.Application.Features.Payment.DTOs;

namespace ClinicHub.Application.Common.Interfaces;

public interface IPaymobService
{
    /// <summary>
    /// Orchestrates the full Paymob wallet payment flow using a single auth token:
    /// 1. Authenticate → 2. Create Order → 3. Generate Payment Key → 4. Pay with Wallet.
    /// Returns the order ID, payment key, and redirect URL for OTP confirmation.
    /// </summary>
    Task<WalletPaymentResultDto> InitiateWalletPaymentAsync(
        decimal amount,
        string currency,
        PaymentBillingData billing,
        string walletPhoneNumber,
        CancellationToken cancellationToken);

    Task<bool> ValidateWebhookAsync(string hmac, IDictionary<string, string> transactionData);
}
