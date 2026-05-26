using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Payment.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ClinicHub.Infrastructure.Services.Paymob;

public class PaymobService : IPaymobService
{
    private readonly HttpClient _httpClient;
    private readonly PaymobSettings _settings;
    private readonly ILogger<PaymobService> _logger;
    private const string BaseUrl = "https://accept.paymob.com";

    public PaymobService(
        HttpClient httpClient,
        IOptions<PaymobSettings> options,
        ILogger<PaymobService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
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
        CancellationToken cancellationToken)
    {
        var amountCents = (int)Math.Round(amount * 100);
        var walletIntegrationId = int.Parse(
            !string.IsNullOrWhiteSpace(_settings.WalletIntegrationId)
                ? _settings.WalletIntegrationId
                : _settings.IntegrationId);

        // Single API call: Create Intention (new unified flow)
        var (clientSecret, intentionId) = await CreateIntentionAsync(
            amountCents, currency, walletIntegrationId,
            billing, walletPhoneNumber, cancellationToken);

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
        CancellationToken cancellationToken)
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
            redirection_url = _settings.RedirectionUrl
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

        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Paymob CreateIntention failed with status {StatusCode}. Response: {Response}",
                response.StatusCode, errorJson);
            throw new BadRequestException(
                $"Paymob intention creation failed ({response.StatusCode}): {errorJson}");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("client_secret", out var csElement))
            throw new InvalidOperationException("Paymob response missing client_secret field");

        var clientSecret = csElement.GetString()
            ?? throw new InvalidOperationException("client_secret is null");

        var intentionId = root.TryGetProperty("id", out var idElement)
            ? idElement.GetString()!
            : root.TryGetProperty("intention_order_id", out var oidElement)
                ? oidElement.GetInt64().ToString()
                : "unknown";

        _logger.LogInformation(
            "Paymob intention created. ID: {IntentionId}, ClientSecret: {ClientSecretPrefix}...",
            intentionId, clientSecret.Substring(0, Math.Min(8, clientSecret.Length)));

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

            if (!isValid)
            {
                _logger.LogWarning(
                    "Paymob HMAC validation failed. Concatenated: {Concatenated}",
                    concatenated);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Paymob webhook HMAC.");
            return false;
        }
    }

    private static string GetValue(IDictionary<string, string> data, params string[] keys)
    {
        foreach (var key in keys)
            if (data.TryGetValue(key, out var value))
                return value?.ToLower() ?? "";
        return "";
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
}