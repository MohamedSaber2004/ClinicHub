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

    public PaymobService(HttpClient httpClient, IOptions<PaymobSettings> options, ILogger<PaymobService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    private async Task<string> GetAuthTokenAsync(CancellationToken cancellationToken)
    {
        var payload = new { api_key = _settings.ApiKey };
        var response = await _httpClient.PostAsync(
            $"{_settings.BaseUrl}/auth/tokens",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("token", out var tokenProp))
            return tokenProp.GetString()!;
        throw new InvalidOperationException("Paymob authentication failed: missing token.");
    }

    public async Task<string> CreateOrderAsync(decimal amount, string currency, CancellationToken cancellationToken)
    {
        var authToken = await GetAuthTokenAsync(cancellationToken);
        var payload = new
        {
            auth_token = authToken,
            amount_cents = (int)Math.Round(amount * 100),
            currency = currency,
            items = Array.Empty<object>()
        };

        var response = await _httpClient.PostAsync(
            $"{_settings.BaseUrl}/ecommerce/orders",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("id", out var idProp))
            return idProp.GetInt64().ToString();
        throw new InvalidOperationException("Paymob order creation failed: missing order id.");
    }

    public async Task<string> GetPaymentKeyAsync(string orderId, decimal amount, PaymentBillingData billing, CancellationToken cancellationToken)
    {
        var authToken = await GetAuthTokenAsync(cancellationToken);
        var payload = new
        {
            auth_token = authToken,
            amount_cents = (int)Math.Round(amount * 100),
            expiration = 3600,
            order_id = orderId,
            billing_data = new
            {
                apartment = string.IsNullOrWhiteSpace(billing.Apartment) ? "NA" : billing.Apartment,
                email = string.IsNullOrWhiteSpace(billing.Email) ? "patient@clinichub.com" : billing.Email,
                floor = string.IsNullOrWhiteSpace(billing.Floor) ? "NA" : billing.Floor,
                first_name = string.IsNullOrWhiteSpace(billing.FirstName) ? "Clinic" : billing.FirstName,
                street = string.IsNullOrWhiteSpace(billing.Street) ? "NA" : billing.Street,
                building = string.IsNullOrWhiteSpace(billing.Building) ? "NA" : billing.Building,
                phone_number = (string.IsNullOrWhiteSpace(billing.PhoneNumber) ? "01000000000" : billing.PhoneNumber).ToPaymobFormat(),
                postal_code = string.IsNullOrWhiteSpace(billing.PostalCode) ? "NA" : billing.PostalCode,
                city = string.IsNullOrWhiteSpace(billing.City) ? "Cairo" : billing.City,
                country = string.IsNullOrWhiteSpace(billing.Country) ? "EG" : billing.Country,
                state = string.IsNullOrWhiteSpace(billing.State) ? "Cairo" : billing.State,
                last_name = string.IsNullOrWhiteSpace(billing.LastName) ? "User" : billing.LastName,
                extra = "NA"
            },
            currency = "EGP",
            integration_id = int.Parse(_settings.IntegrationId)
        };

        var response = await _httpClient.PostAsync(
            $"{_settings.BaseUrl}/acceptance/payment_keys",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("token", out var tokenProp))
            return tokenProp.GetString()!;
        throw new InvalidOperationException("Paymob payment key retrieval failed: missing token.");
    }

    public async Task<string> PayWithWalletAsync(string paymentToken, string phoneNumber, CancellationToken cancellationToken)
    {
        // Format phone number for Paymob: Egyptian wallets require the 11-digit local format (01XXXXXXXXX).
        // The ToPaymobFormat extension now handles this conversion correctly.
        var formattedPhoneNumber = phoneNumber.ToPaymobFormat();

        _logger.LogInformation("Processing wallet payment with formatted phone: {PhoneNumber} (original: {OriginalPhone})", formattedPhoneNumber, phoneNumber);

        var payload = new
        {
            source = new
            {
                identifier = formattedPhoneNumber,
                subtype = "WALLET",
                type = "wallet"
            },
            payment_token = paymentToken
        };

        var response = await _httpClient.PostAsync(
            $"{_settings.BaseUrl}/acceptance/payments/pay",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("Paymob wallet payment response: Status={StatusCode}, Body={Response}", response.StatusCode, json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Check for immediate failure: if not success and not pending, it's a hard fail
        bool hasSuccess = root.TryGetProperty("success", out var successProp) && IsTruthy(successProp);
        bool isPending = root.TryGetProperty("pending", out var pendingProp) && IsTruthy(pendingProp);
        bool errorOccured = root.TryGetProperty("error_occured", out var errOccured) && IsTruthy(errOccured);

        // Try to extract URL early - if we have a URL, it's usually a good sign even if flags are weird
        string? foundUrl = FindUrl(root);

        if (errorOccured || (!hasSuccess && !isPending && string.IsNullOrEmpty(foundUrl)))
        {
            string errorMessage = ExtractErrorMessage(root);

            _logger.LogError("Paymob wallet payment failed: {Message}. Full Response: {Response}", errorMessage, json);
            throw new BadRequestException($"Payment Failed: {errorMessage}");
        }

        if (!response.IsSuccessStatusCode && string.IsNullOrEmpty(foundUrl))
        {
            _logger.LogError("Paymob wallet payment failed with status {StatusCode}. Response: {Response}", response.StatusCode, json);
            // Include status code in message for easier debugging
            throw new BadRequestException($"Paymob API Error (Status {response.StatusCode}): {ExtractErrorMessage(root)}");
        }

        if (!string.IsNullOrEmpty(foundUrl))
            return foundUrl;

        // 3. If we get here and have no URL, log ALL keys so we can see what Paymob sent
        var keys = string.Join(", ", root.EnumerateObject().Select(p => p.Name));
        _logger.LogError("Paymob response did not contain a URL. Available keys: {Keys}. Full response: {Response}", keys, json);

        throw new InvalidOperationException($"Paymob wallet payment failed. No URL found in response. Available fields: {keys}");
    }

    private static string ExtractErrorMessage(JsonElement root)
    {
        // Priority 1: Look in 'data' object (common in Paymob)
        if (root.TryGetProperty("data", out var data))
        {
            if (data.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) return m.GetString()!;
            if (data.TryGetProperty("explanation", out var e) && e.ValueKind == JsonValueKind.String) return e.GetString()!;
        }

        // Priority 2: Look in 'source_data' (wallet specific)
        if (root.TryGetProperty("source_data", out var sourceData))
        {
            if (sourceData.TryGetProperty("message", out var sm) && sm.ValueKind == JsonValueKind.String) return sm.GetString()!;
        }

        // Priority 3: Root level standard fields
        if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String) return msg.GetString()!;
        if (root.TryGetProperty("detail", out var det) && det.ValueKind == JsonValueKind.String) return det.GetString()!;
        if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String) return err.GetString()!;

        // Priority 4: Look for any property containing 'message' or 'error'
        foreach (var prop in root.EnumerateObject())
        {
            if ((prop.Name.Contains("message", StringComparison.OrdinalIgnoreCase) || 
                 prop.Name.Contains("error", StringComparison.OrdinalIgnoreCase)) && 
                prop.Value.ValueKind == JsonValueKind.String)
            {
                return prop.Value.GetString()!;
            }
        }

        return "Payment declined or failed";
    }

    private static bool IsTruthy(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => element.GetString()?.ToLower() switch
            {
                "true" => true,
                "1" => true,
                "yes" => true,
                _ => false
            },
            JsonValueKind.Number => element.TryGetInt32(out var val) && val == 1,
            _ => false
        };
    }

    private static string? FindUrl(JsonElement root)
    {
        // 1. Try to find ANY field that contains "url" and has a string value at root
        foreach (var prop in root.EnumerateObject())
        {
            if (prop.Name.Contains("url", StringComparison.OrdinalIgnoreCase) && prop.Value.ValueKind == JsonValueKind.String)
                return prop.Value.GetString();
        }

        // 2. Check inside 'data' object if it exists
        if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in dataProp.EnumerateObject())
            {
                if (prop.Name.Contains("url", StringComparison.OrdinalIgnoreCase) && prop.Value.ValueKind == JsonValueKind.String)
                    return prop.Value.GetString();
            }
        }

        return null;
    }

    public async Task<bool> ValidateWebhookAsync(string hmac, IDictionary<string, string> transactionData)
    {
        try
        {
            // Paymob HMAC (Transaction Webhook & Callback) uses 21 specific fields in this EXACT order:
            // amount_cents, created_at, currency, error_occured, has_parent_transaction, id, integration_id, 
            // is_3d_secure, is_auth, is_capture, is_refunded, is_standalone_payment, is_voided, 
            // order.id (or order), owner, pending, source_data.pan, source_data.sub_type, source_data.type, success
            
            // We handle both dot notation (callbacks) and underscore notation (some webhooks/flattened data)
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

            var concatenated = amount_cents + created_at + currency + error_occured + has_parent_transaction +
                               id + integration_id + is_3d_secure + is_auth + is_capture + is_refunded +
                               is_standalone_payment + is_voided + order_id + owner + pending +
                               source_pan + source_sub_type + source_type + success;

            var computed = ComputeHmacSha256(_settings.HmacSecret, concatenated);
            var isValid = string.Equals(computed, hmac, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("Paymob HMAC validation failed. Concatenated string: {Concatenated}", concatenated);
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
        {
            if (data.TryGetValue(key, out var value))
                return value?.ToLower() ?? "";
        }
        return "";
    }

    private static string ComputeHmacSha256(string secret, string message)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}