using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Payment.DTOs;
using ClinicHub.Application.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ClinicHub.Infrastructure.Services.Paymob;

public class PaymobService : IPaymobService
{
    private readonly HttpClient _httpClient;
    private readonly PaymobSettings _settings;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly ILogger<PaymobService> _logger;
    private const string BaseUrl = "https://accept.paymob.com";

    public PaymobService(
        HttpClient httpClient,
        IOptions<PaymobSettings> options,
        IStringLocalizer<Messages> localizer,
        ILogger<PaymobService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _localizer = localizer;
        _logger = logger;
    }

    private int ResolveIntegrationId(string? rawId, string fallbackRaw)
    {
        var candidate = !string.IsNullOrWhiteSpace(rawId) ? rawId : fallbackRaw;
        if (!int.TryParse(candidate, out var id))
        {
            _logger.LogError("Paymob IntegrationId '{Raw}' is not a valid integer. Check PaymobSettings IntegrationId/WalletIntegrationId in appsettings. Placeholder values like YOUR_... will fail.", candidate);
            throw new BadRequestException($"Paymob IntegrationId '{candidate}' is not configured. Check appsettings PaymobSettings.");
        }
        return id;
    }

    /// <inheritdoc />
    /// <summary>
    /// Initiates a wallet payment using Paymob's Intention API (Unified Checkout / Flash).
    /// This is the new single-step flow replacing the legacy auth → order → payment_key → pay approach.
    /// </summary>
    public async Task<WalletPaymentResultDto> InitiateWalletPaymentAsync(
        decimal amount,
        string currency,
        PaymentBillingData billing,
        string walletPhoneNumber,
        CancellationToken cancellationToken,
        string? redirectionUrl = null)
    {
        var amountCents = (int)Math.Round(amount * 100);
        var walletIntegrationId = ResolveIntegrationId(_settings.WalletIntegrationId, _settings.IntegrationId);

        // Single API call: Create Intention (new unified flow)
        var (clientSecret, intentionId) = await CreateIntentionAsync(
            amountCents, currency, walletIntegrationId,
            billing, walletPhoneNumber, cancellationToken, redirectionUrl);

        // Build redirect URL using Public Key + Client Secret
        var redirectUrl = $"{BaseUrl}/unifiedcheckout/" +
                          $"?publicKey={_settings.PublicKey}" +
                          $"&clientSecret={clientSecret}";

        return new WalletPaymentResultDto
        {
            OrderId = intentionId,
            PaymentKey = clientSecret,
            RedirectUrl = redirectUrl
        };
    }

    /// <summary>
    /// Initiates a hosted-checkout payment (unified checkout page) using Paymob's Intention API.
    /// Used when an appointment is accepted and the patient must be sent a payment link —
    /// the default (card) integration is used so the patient can complete payment from the hosted page.
    /// </summary>
    public async Task<WalletPaymentResultDto> InitiateCheckoutPaymentAsync(
        decimal amount,
        string currency,
        PaymentBillingData billing,
        CancellationToken cancellationToken,
        string? redirectionUrl = null)
    {
        var amountCents = (int)Math.Round(amount * 100);
        var integrationId = ResolveIntegrationId(_settings.IntegrationId, _settings.IntegrationId);

        // Single API call: Create Intention (new unified flow)
        var (clientSecret, intentionId) = await CreateIntentionAsync(
            amountCents, currency, integrationId,
            billing, billing.PhoneNumber ?? "", cancellationToken, redirectionUrl);

        // Build redirect URL using Public Key + Client Secret
        var redirectUrl = $"{BaseUrl}/unifiedcheckout/" +
                          $"?publicKey={_settings.PublicKey}" +
                          $"&clientSecret={clientSecret}";

        return new WalletPaymentResultDto
        {
            OrderId = intentionId,
            PaymentKey = clientSecret,
            RedirectUrl = redirectUrl
        };
    }

    /// <summary>
    /// Creates a payment intention using Paymob's new Intention API.
    /// Returns (client_secret, intention_id) tuple.
    /// </summary>
    private async Task<(string clientSecret, string intentionId)> CreateIntentionAsync(
        int amountCents,
        string currency,
        int integrationId,
        PaymentBillingData billing,
        string walletPhoneNumber,
        CancellationToken cancellationToken,
        string? redirectionUrl = null)
    {
        var payload = new
        {
            amount = amountCents,
            currency = currency,
            payment_methods = new[] { integrationId },
            items = new[]
            {
                new
                {
                    name = "ClinicHub Appointment",
                    amount = amountCents,
                    description = "Medical appointment booking",
                    quantity = 1
                }
            },
            billing_data = new
            {
                first_name = string.IsNullOrWhiteSpace(billing.FirstName)
                    ? "Clinic"
                    : billing.FirstName,
                last_name = string.IsNullOrWhiteSpace(billing.LastName)
                    ? "User"
                    : billing.LastName,
                email = string.IsNullOrWhiteSpace(billing.Email)
                    ? "patient@clinichub.com"
                    : billing.Email,
                phone_number = (string.IsNullOrWhiteSpace(billing.PhoneNumber)
                    ? walletPhoneNumber
                    : billing.PhoneNumber).ToPaymobFormat(),
                apartment = string.IsNullOrWhiteSpace(billing.Apartment) ? "NA" : billing.Apartment,
                floor = string.IsNullOrWhiteSpace(billing.Floor) ? "NA" : billing.Floor,
                street = string.IsNullOrWhiteSpace(billing.Street) ? "NA" : billing.Street,
                building = string.IsNullOrWhiteSpace(billing.Building) ? "NA" : billing.Building,
                postal_code = string.IsNullOrWhiteSpace(billing.PostalCode) ? "NA" : billing.PostalCode,
                city = string.IsNullOrWhiteSpace(billing.City) ? "Cairo" : billing.City,
                country = string.IsNullOrWhiteSpace(billing.Country) ? "EG" : billing.Country,
                state = string.IsNullOrWhiteSpace(billing.State) ? "Cairo" : billing.State
            },
            notification_url = _settings.WebhookUrl,
            redirection_url = !string.IsNullOrWhiteSpace(redirectionUrl) ? redirectionUrl : _settings.RedirectionUrl
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/intention/")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        // New auth method: Secret Key in Authorization header
        request.Headers.Add("Authorization", $"Token {_settings.SecretKey}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob CreateIntention failed: integrationId={IntegrationId} status={Status} body={Body} amountCents={Amount} currency={Currency}",
                integrationId, (int)response.StatusCode, responseBody, amountCents, currency);
            // Surface Paymob's detail when available (e.g., 'Integration ID does not exist')
            var detail = TryExtractDetail(responseBody);
            throw new BadRequestException(
                string.IsNullOrWhiteSpace(detail)
                    ? _localizer[LocalizationKeys.PaymentMessages.PaymobOrderFailed.Value]
                    : $"{_localizer[LocalizationKeys.PaymentMessages.PaymobOrderFailed.Value]}: {detail}");
        }

        var json = responseBody;
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("client_secret", out var csElement))
            throw new InvalidOperationException(_localizer[LocalizationKeys.PaymentMessages.PaymobKeyFailed.Value]);

        var clientSecret = csElement.GetString()
            ?? throw new InvalidOperationException(_localizer[LocalizationKeys.PaymentMessages.PaymobKeyFailed.Value]);

        var intentionId = root.TryGetProperty("intention_order_id", out var oidElement)
            ? oidElement.GetInt64().ToString()
            : root.TryGetProperty("id", out var idElement)
                ? idElement.GetString()!
                : "unknown";

        return (clientSecret, intentionId);
    }

    /// <summary>
    /// Validates webhook HMAC using SHA512 (Paymob requirement).
    /// </summary>
    public async Task<bool> ValidateWebhookAsync(string hmac, IDictionary<string, string> transactionData)
    {
        try
        {
            var amount_cents = GetValue(transactionData, "amount_cents");
            var created_at = GetValue(transactionData, "created_at");
            var currency = GetValue(transactionData, "currency");
            var error_occured = GetValue(transactionData, "error_occured");
            var has_parent_transaction = GetValue(transactionData, "has_parent_transaction");
            var id = GetValue(transactionData, "id");
            var integration_id = GetValue(transactionData, "integration_id");
            var is_3d_secure = GetValue(transactionData, "is_3d_secure");
            var is_auth = GetValue(transactionData, "is_auth");
            var is_capture = GetValue(transactionData, "is_capture");
            var is_refunded = GetValue(transactionData, "is_refunded");
            var is_standalone_payment = GetValue(transactionData, "is_standalone_payment");
            var is_voided = GetValue(transactionData, "is_voided");
            var order_id = GetValue(transactionData, "order.id", "order_id", "order");
            var owner = GetValue(transactionData, "owner");
            var pending = GetValue(transactionData, "pending");
            var source_pan = GetValue(transactionData, "source_data.pan", "source_data_pan");
            var source_sub_type = GetValue(transactionData, "source_data.sub_type", "source_data_sub_type");
            var source_type = GetValue(transactionData, "source_data.type", "source_data_type");
            var success = GetValue(transactionData, "success");

            var concatenated = amount_cents + created_at + currency + error_occured +
                               has_parent_transaction + id + integration_id + is_3d_secure +
                               is_auth + is_capture + is_refunded + is_standalone_payment +
                               is_voided + order_id + owner + pending +
                               source_pan + source_sub_type + source_type + success;

            // ✅ Use SHA512 (Paymob requirement), NOT SHA256
            var computed = ComputeHmacSha512(_settings.HmacSecret, concatenated);
            var isValid = string.Equals(computed, hmac, StringComparison.OrdinalIgnoreCase);

            return isValid;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<PaymobOrderStatusDto> GetOrderPaymentStatusAsync(string orderId, CancellationToken cancellationToken)
    {
        var result = new PaymobOrderStatusDto();

        if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(orderId))
            return result;

        try
        {
            // 1. Legacy auth token (valid for 1 hour)
            var authPayload = new { api_key = _settings.ApiKey };
            var authRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/auth/tokens")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(authPayload),
                    Encoding.UTF8,
                    "application/json")
            };

            var authResponse = await _httpClient.SendAsync(authRequest, cancellationToken);
            if (!authResponse.IsSuccessStatusCode)
                return result;

            var authJson = await authResponse.Content.ReadAsStringAsync(cancellationToken);
            using var authDoc = JsonDocument.Parse(authJson);
            if (!authDoc.RootElement.TryGetProperty("token", out var tokenElement))
                return result;

            var token = tokenElement.GetString();
            if (string.IsNullOrWhiteSpace(token))
                return result;

            // 2. Order inquiry
            var orderResponse = await _httpClient.GetAsync(
                $"{BaseUrl}/api/ecommerce/orders/{orderId}?token={Uri.EscapeDataString(token)}",
                cancellationToken);

            if (!orderResponse.IsSuccessStatusCode)
                return result;

            var orderJson = await orderResponse.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(orderJson);
            var root = doc.RootElement;

            result.Found = true;
            result.AmountCents = root.TryGetProperty("amount_cents", out var amountEl) && amountEl.TryGetInt64(out var a) ? a : 0;
            result.PaidAmountCents = root.TryGetProperty("paid_amount_cents", out var paidEl) && paidEl.TryGetInt64(out var p) ? p : 0;
            result.Paid = result.AmountCents > 0 && result.PaidAmountCents >= result.AmountCents;

            return result;
        }
        catch (Exception)
        {
            // Inquiry is best-effort; treat any failure as "unknown / not found"
            // so the caller falls back to the webhook as the source of truth.
            return new PaymobOrderStatusDto();
        }
    }

    private static string GetValue(IDictionary<string, string> data, params string[] keys)
    {
        foreach (var key in keys)
            if (data.TryGetValue(key, out var value))
                return value ?? "";
        return "";
    }

    private static string? TryExtractDetail(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("detail", out var d)) return d.GetString();
            if (doc.RootElement.TryGetProperty("message", out var m)) return m.GetString();
            if (doc.RootElement.TryGetProperty("error", out var e)) return e.GetString();
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Computes HMAC-SHA512 hash (Paymob requirement for webhook validation).
    /// </summary>
    private static string ComputeHmacSha512(string secret, string message)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512(
            Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public async Task<RefundResultDto> RefundTransactionAsync(string transactionId, decimal amount, CancellationToken cancellationToken)
    {
        if (!long.TryParse(transactionId, out var txId))
        {
            return new RefundResultDto
            {
                Success = false,
                Message = _localizer[LocalizationKeys.PaymentMessages.InvalidTransactionId]
            };
        }

        var amountCents = (int)Math.Round(amount * 100);

        var payload = new
        {
            transaction_id = txId,
            amount_cents = amountCents
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/acceptance/void_refund/refund")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Add("Authorization", $"Token {_settings.SecretKey}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new RefundResultDto
            {
                Success = false,
                Message = _localizer[LocalizationKeys.PaymentMessages.RefundFailed]
            };
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var refundId = root.TryGetProperty("id", out var idElement)
            ? idElement.GetInt64().ToString()
            : null;

        return new RefundResultDto
        {
            Success = true,
            RefundId = refundId,
            Message = _localizer[LocalizationKeys.PaymentMessages.RefundSuccess]
        };
    }
}
