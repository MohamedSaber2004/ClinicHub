using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookRequestDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("hmac")]
    public string Hmac { get; set; } = string.Empty;

    [JsonPropertyName("obj")]
    public PaymobTransaction Transaction { get; set; } = new();
}

public class PaymobTransaction
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("is_auth")]
    public bool IsAuth { get; set; }

    [JsonPropertyName("is_capture")]
    public bool IsCapture { get; set; }

    [JsonPropertyName("is_standalone_payment")]
    public bool IsStandalonePayment { get; set; }

    [JsonPropertyName("is_voided")]
    public bool IsVoided { get; set; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; set; }

    [JsonPropertyName("is_3d_secure")]
    public bool Is3DSecure { get; set; }

    [JsonPropertyName("integration_id")]
    public int IntegrationId { get; set; }

    [JsonPropertyName("profile_id")]
    public int ProfileId { get; set; }

    [JsonPropertyName("owner")]
    public int Owner { get; set; }

    [JsonPropertyName("has_parent_transaction")]
    public bool HasParentTransaction { get; set; }

    [JsonPropertyName("order")]
    public PaymobOrder Order { get; set; } = new();

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("source_data")]
    public PaymobSourceData SourceData { get; set; } = new();

    [JsonPropertyName("api_source")]
    public string ApiSource { get; set; } = string.Empty;

    [JsonPropertyName("error_occured")]
    public bool ErrorOccurred { get; set; }

    [JsonPropertyName("is_settled")]
    public bool IsSettled { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

public class PaymobOrder
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}

public class PaymobSourceData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("sub_type")]
    public string SubType { get; set; } = string.Empty;

    [JsonPropertyName("pan")]
    public string Pan { get; set; } = string.Empty;
}
