using ClinicHub.Application.Features.Payment.DTOs;

namespace ClinicHub.Application.Common.Interfaces;

public interface IPaymobService
{
    Task<WalletPaymentResultDto> InitiateWalletPaymentAsync(
        decimal amount,
        string currency,
        PaymentBillingData billing,
        string walletPhoneNumber,
        CancellationToken cancellationToken,
        string? redirectionUrl = null);

    Task<bool> ValidateWebhookAsync(string hmac, IDictionary<string, string> transactionData);

    Task<RefundResultDto> RefundTransactionAsync(string transactionId, decimal amount, CancellationToken cancellationToken);
}
