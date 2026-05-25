using ClinicHub.Application.Features.Payment.DTOs;

namespace ClinicHub.Application.Common.Interfaces;

public interface IPaymobService
{
    Task<string> CreateOrderAsync(decimal amount, string currency, CancellationToken cancellationToken);
    Task<string> GetPaymentKeyAsync(string orderId, decimal amount, PaymentBillingData billing, CancellationToken cancellationToken);
    Task<string> PayWithWalletAsync(string paymentToken, string phoneNumber, CancellationToken cancellationToken);
    Task<bool> ValidateWebhookAsync(string hmac, IDictionary<string, string> transactionData);
}
