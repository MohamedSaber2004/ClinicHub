using Asp.Versioning;
using ClinicHub.API.Routes;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.PayoutWebhook.Commands.ProcessPayoutWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ClinicHub.API.Controllers.Version1;

/// <summary>
/// Paymob's payout callback. It carries no signature, so the URL itself holds the secret
/// segment and the payload is only a hint — settlement still comes from the inquiry API.
/// </summary>
[ApiVersion("1.0")]
[AllowAnonymous]
[ApiController]
public class PayoutWebhookController : BaseApiController
{
    private readonly PayoutSettings _payoutSettings;
    private readonly ILogger<PayoutWebhookController> _logger;

    public PayoutWebhookController(
        IMediator mediator,
        IOptions<PayoutSettings> payoutSettings,
        ILogger<PayoutWebhookController> logger)
        : base(mediator)
    {
        _payoutSettings = payoutSettings.Value;
        _logger = logger;
    }

    [HttpPost]
    [Route(ApiRoutes.PayoutWebhook.Receive)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Receive(string token, CancellationToken ct)
    {
        if (!IsAuthorized(token))
            return NotFound();

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var raw = await reader.ReadToEndAsync(ct);

        var payload = TryParse(raw);
        var transactionId = ReadString(payload, "transaction_id") ?? ReadString(payload, "id");
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            _logger.LogWarning("Payout callback carried no transaction id. Body: {Body}", raw);
            return Ok(false);
        }

        await _mediator.Send(new ProcessPayoutWebhookCommand
        {
            TransactionId = transactionId,
            Issuer = ReadString(payload, "issuer"),
            Amount = ReadDecimal(payload, "amount"),
            StatusCode = ReadString(payload, "status_code"),
            DisbursementStatus = ReadString(payload, "disbursement_status"),
            RawPayload = raw
        }, ct);

        return Ok(true);
    }

    private bool IsAuthorized(string? token)
    {
        if (string.IsNullOrWhiteSpace(_payoutSettings.CallbackSecret) || string.IsNullOrEmpty(token))
            return false;

        var expected = Encoding.UTF8.GetBytes(_payoutSettings.CallbackSecret);
        var provided = Encoding.UTF8.GetBytes(token);
        return expected.Length == provided.Length && CryptographicOperations.FixedTimeEquals(expected, provided);
    }

    private static JsonElement? TryParse(string raw)
    {
        try
        {
            return JsonDocument.Parse(raw).RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement? root, string name)
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in root.Value.EnumerateObject())
        {
            if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                continue;

            return property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetRawText(),
                _ => null
            };
        }

        return null;
    }

    private static decimal? ReadDecimal(JsonElement? root, string name)
    {
        var text = ReadString(root, name);
        return decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }
}
