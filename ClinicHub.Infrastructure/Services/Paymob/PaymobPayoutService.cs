using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ClinicHub.Infrastructure.Services.Paymob;

/// <summary>
/// Paymob Payouts client. Money leaves the platform here, so the only two answers this
/// class is allowed to give are "Paymob took the transaction" and "Paymob refused it";
/// anything else — timeout, 5xx, unparseable body — is <see cref="PayoutOutcome.Unknown"/>
/// and must be settled by inquiry before a retry.
/// </summary>
public class PaymobPayoutService : IPayoutService
{
    private const string TokenCacheKey = "payouts:access_token";
    private static readonly TimeSpan TokenExpirySafetyMargin = TimeSpan.FromMinutes(5);

    private readonly HttpClient _httpClient;
    private readonly PayoutSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PaymobPayoutService> _logger;

    public PaymobPayoutService(
        HttpClient httpClient,
        IOptions<PayoutSettings> options,
        IMemoryCache cache,
        ILogger<PaymobPayoutService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PayoutBudgetResult> GetBudgetAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAuthorizedAsync(
            HttpMethod.Get, $"{_settings.BaseUrl}/api/secure/budget/inquire/", null, cancellationToken);

        if (response is null)
            return FailedBudget("The payout service did not answer.");

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Payout budget inquiry failed: {Status} {Body}", (int)response.StatusCode, body);
            return FailedBudget($"HTTP {(int)response.StatusCode}");
        }

        var root = TryParse(body);
        if (root is null)
            return FailedBudget("Unreadable budget response.");

        return new PayoutBudgetResult
        {
            Success = true,
            Balance = ReadBalance(root.Value),
            Currency = ReadString(root.Value, "currency") ?? "EGP",
            StatusCode = ReadString(root.Value, "status_code"),
            StatusDescription = ReadString(root.Value, "status_description")
        };
    }

    /// <inheritdoc />
    public async Task<PayoutDisbursementResult> DisburseBankAsync(
        PayoutBankDisbursementRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, string>
        {
            ["issuer"] = "bank_card",
            ["amount"] = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            ["full_name"] = request.FullName,
            ["bank_card_number"] = request.BankCardNumber,
            ["bank_code"] = request.BankCode,
            ["bank_transaction_type"] = request.BankTransactionType,
            ["national_id"] = request.NationalId,
            ["client_reference_id"] = request.ClientReferenceId.ToString()
        };

        HttpContent content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage? response;
        try
        {
            response = await SendAuthorizedAsync(
                HttpMethod.Post, $"{_settings.BaseUrl}/api/secure/disburse/", content, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient timeout: Paymob may well have taken the disbursement.
            _logger.LogWarning(
                "Payout disburse timed out for reference {Reference}; outcome unknown.",
                request.ClientReferenceId);
            return Unknown("The disbursement request timed out.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex,
                "Payout disburse transport failure for reference {Reference}; outcome unknown.",
                request.ClientReferenceId);
            return Unknown(ex.Message);
        }

        if (response is null)
            return Unknown("The payout service did not answer.");

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var root = TryParse(body);

        var transactionId = root is null
            ? null
            : ReadString(root.Value, "transaction_id") ?? ReadString(root.Value, "id");
        var statusCode = root is null ? null : ReadString(root.Value, "status_code");
        var statusDescription = root is null ? null : ReadString(root.Value, "status_description");

        if (response.IsSuccessStatusCode && transactionId is not null)
        {
            return new PayoutDisbursementResult
            {
                Outcome = PayoutOutcome.Accepted,
                TransactionId = transactionId,
                StatusCode = statusCode,
                StatusDescription = statusDescription,
                DisbursementStatus = root is null ? null : ReadString(root.Value, "disbursement_status")
            };
        }

        // A 2xx without a transaction id means Paymob never opened one, so the amount is safe
        // to release; a 4xx/5xx with an id is the opposite and must be treated as unknown.
        if (transactionId is not null)
        {
            _logger.LogWarning(
                "Payout disburse returned HTTP {Status} with transaction {TransactionId}; treated as unknown.",
                (int)response.StatusCode, transactionId);
            return new PayoutDisbursementResult
            {
                Outcome = PayoutOutcome.Unknown,
                TransactionId = transactionId,
                StatusCode = statusCode,
                StatusDescription = statusDescription
            };
        }

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity
            or HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(
                "Payout disburse rejected for reference {Reference}: {Status} {Body}",
                request.ClientReferenceId, (int)response.StatusCode, body);
            return new PayoutDisbursementResult
            {
                Outcome = PayoutOutcome.Rejected,
                StatusCode = statusCode,
                StatusDescription = statusDescription ?? body
            };
        }

        _logger.LogWarning(
            "Payout disburse outcome unclear for reference {Reference}: HTTP {Status} {Body}",
            request.ClientReferenceId, (int)response.StatusCode, body);
        return Unknown(statusDescription ?? $"HTTP {(int)response.StatusCode}");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PayoutTransactionStatus>> InquireAsync(
        IReadOnlyCollection<string> transactionIds,
        CancellationToken cancellationToken = default)
    {
        if (transactionIds.Count == 0)
            return Array.Empty<PayoutTransactionStatus>();

        return await InquireCoreAsync(
            $"{_settings.BaseUrl}/api/secure/transaction/inquire/",
            "transactions_ids_list",
            transactionIds,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PayoutTransactionStatus>> InquireByReferenceAsync(
        IReadOnlyCollection<string> clientReferenceIds,
        CancellationToken cancellationToken = default)
    {
        if (clientReferenceIds.Count == 0)
            return Array.Empty<PayoutTransactionStatus>();

        return await InquireCoreAsync(
            $"{_settings.BaseUrl}/api/secure/transaction/inquire/by-reference/",
            "references_list",
            clientReferenceIds,
            cancellationToken);
    }

    private async Task<IReadOnlyList<PayoutTransactionStatus>> InquireCoreAsync(
        string url,
        string listField,
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object> { [listField] = ids };
        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await SendAuthorizedAsync(HttpMethod.Post, url, content, cancellationToken);
        if (response is null)
            return Array.Empty<PayoutTransactionStatus>();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Payout inquiry {Url} failed: {Status} {Body}",
                url, (int)response.StatusCode, body);
            return Array.Empty<PayoutTransactionStatus>();
        }

        var root = TryParse(body);
        if (root is null)
            return Array.Empty<PayoutTransactionStatus>();

        return EnumerateTransactions(root.Value);
    }

    private async Task<HttpResponseMessage?> SendAuthorizedAsync(
        HttpMethod method,
        string url,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);
        if (token is null)
            return null;

        var response = await ExecuteAsync(method, url, content, token, cancellationToken);
        if (response is not null && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            _cache.Remove(TokenCacheKey);
            token = await GetTokenAsync(cancellationToken);
            if (token is null)
                return null;
            response = await ExecuteAsync(method, url, content, token, cancellationToken);
        }

        return response;
    }

    private async Task<HttpResponseMessage?> ExecuteAsync(
        HttpMethod method,
        string url,
        HttpContent? content,
        string token,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(method, url);
            if (content is not null)
            {
                // A reused HttpContent cannot be sent twice, so the body is rebuilt per attempt.
                var bytes = await content.ReadAsByteArrayAsync(cancellationToken);
                request.Content = new ByteArrayContent(bytes);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Payout call to {Url} failed at transport level.", url);
            return null;
        }
    }

    private async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<string>(TokenCacheKey, out var cached) && !string.IsNullOrWhiteSpace(cached))
            return cached;

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["username"] = _settings.Username,
            ["password"] = _settings.Password
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/secure/o/token/")
            {
                Content = new FormUrlEncodedContent(form)
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Payout token request failed: {Status} {Body}", (int)response.StatusCode, body);
                return null;
            }

            var root = TryParse(body);
            var token = root is null ? null : ReadString(root.Value, "access_token");
            if (token is null)
            {
                _logger.LogError("Payout token response carried no access_token: {Body}", body);
                return null;
            }

            var expiresIn = root is not null && TryReadInt(root.Value, "expires_in", out var seconds)
                ? TimeSpan.FromSeconds(seconds)
                : TimeSpan.FromMinutes(30);

            var lifetime = expiresIn > TokenExpirySafetyMargin
                ? expiresIn - TokenExpirySafetyMargin
                : TimeSpan.FromSeconds(30);

            _cache.Set(TokenCacheKey, token, lifetime);
            return token;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "Payout token request threw.");
            return null;
        }
    }

    private static IReadOnlyList<PayoutTransactionStatus> EnumerateTransactions(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().Select(MapTransaction).Where(t => t.TransactionId.Length > 0).ToList();

        if (root.ValueKind != JsonValueKind.Object)
            return Array.Empty<PayoutTransactionStatus>();

        foreach (var name in new[] { "results", "data", "transactions" })
        {
            if (root.TryGetProperty(name, out var list) && list.ValueKind == JsonValueKind.Array)
                return list.EnumerateArray().Select(MapTransaction).Where(t => t.TransactionId.Length > 0).ToList();
        }

        return new[] { MapTransaction(root) };
    }

    private static PayoutTransactionStatus MapTransaction(JsonElement element) => new()
    {
        TransactionId = ReadString(element, "transaction_id") ?? ReadString(element, "id") ?? string.Empty,
        ClientReferenceId = ReadString(element, "client_reference_id") ?? ReadString(element, "client_reference"),
        DisbursementStatus = ReadString(element, "disbursement_status") ?? ReadString(element, "status"),
        StatusCode = ReadString(element, "status_code"),
        StatusDescription = ReadString(element, "status_description"),
        Amount = ReadDecimal(element)
    };

    private static decimal ReadBalance(JsonElement root)
    {
        var direct = ReadDecimal(root);
        if (direct is > 0)
            return direct.Value;

        // Some deployments only send "Your current budget is 888.25 LE".
        var description = ReadString(root, "status_description");
        if (description is not null)
        {
            var match = Regex.Match(description, @"\d+(?:[.,]\d+)?");
            if (match.Success && decimal.TryParse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

        return 0m;
    }

    private static decimal? ReadDecimal(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var name in new[] { "current_budget", "current_balance", "balance", "amount" })
        {
            if (!root.TryGetProperty(name, out var element))
                continue;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var numeric))
                return numeric;

            if (element.ValueKind == JsonValueKind.String &&
                decimal.TryParse(element.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var text))
                return text;
        }

        return null;
    }

    private static bool TryReadInt(JsonElement root, string name, out int value)
    {
        value = 0;
        if (!root.TryGetProperty(name, out var element))
            return false;

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out value))
            return true;

        return element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out value);
    }

    /// <summary>Paymob mixes casing across payout endpoints, so property lookup is case-insensitive.</summary>
    private static string? ReadString(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                continue;

            return property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        return null;
    }

    private static JsonElement? TryParse(string body)
    {
        try
        {
            return JsonDocument.Parse(body).RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static PayoutDisbursementResult Unknown(string? reason) => new()
    {
        Outcome = PayoutOutcome.Unknown,
        StatusDescription = reason
    };

    private static PayoutBudgetResult FailedBudget(string reason) => new()
    {
        Success = false,
        StatusDescription = reason
    };
}
