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

    Task<WalletPaymentResultDto> InitiateCheckoutPaymentAsync(
        decimal amount,
        string currency,
        PaymentBillingData billing,
        CancellationToken cancellationToken,
        string? redirectionUrl = null);

    Task<bool> ValidateWebhookAsync(string hmac, IDictionary<string, string> transactionData);

    /// <summary>
    /// Asks Paymob directly whether an order has been paid (server-to-server inquiry).
    /// Used as a fallback when the webhook has not arrived yet.
    /// </summary>
    Task<PaymobOrderStatusDto> GetOrderPaymentStatusAsync(string orderId, CancellationToken cancellationToken);

    Task<RefundResultDto> RefundTransactionAsync(string transactionId, decimal amount, CancellationToken cancellationToken);
}
