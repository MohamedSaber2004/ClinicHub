using Asp.Versioning;
using ClinicHub.Application.Common.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;

namespace ClinicHub.API.Controllers.Version1;

/// <summary>
/// Helper controller for testing webhook payloads.
/// This controller is only available in development environment.
/// </summary>
[ApiVersion("1.0")]
[ApiExplorerSettings(IgnoreApi = false)]
public class WebhookTestHelperController : ControllerBase
{
    private readonly PaymobSettings _paymobSettings;

    public WebhookTestHelperController(IOptions<PaymobSettings> paymobSettings)
    {
        _paymobSettings = paymobSettings.Value;
    }

    /// <summary>
    /// Generates a valid HMAC for testing webhook payloads.
    /// This endpoint helps you test the payment webhook without actual Paymob requests.
    /// </summary>
    /// <param name="transactionData">Dictionary of transaction data to compute HMAC for</param>
    /// <returns>A valid HMAC that can be used to test the webhook endpoint</returns>
    [HttpPost("api/v1/payments/webhook/generate-hmac")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GenerateHmac([FromBody] Dictionary<string, string> transactionData)
    {
        if (transactionData == null || transactionData.Count == 0)
            return BadRequest(new { error = "Transaction data is required" });

        var hmac = ComputeHmacSha256(_paymobSettings.HmacSecret, transactionData);
        
        return Ok(new
        {
            hmac = hmac,
            message = "Use this HMAC value in your webhook test request",
            exampleRequest = new
            {
                hmac = hmac,
                transactionData = transactionData
            }
        });
    }

    private static string ComputeHmacSha256(string secret, IDictionary<string, string> transactionData)
    {
        var concatenated = string.Concat(transactionData.Values);
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
