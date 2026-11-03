using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

/// <summary>
/// Raw Paymob payout callback, kept before anything is applied. Paymob documents no HMAC for
/// these callbacks, so the payload is only a hint: the status is always re-inquired before
/// money is settled in our books, and the unique transaction id makes a replay a no-op.
/// </summary>
public class PayoutWebhookEvent : BaseEntity<Guid>
{
    public string PaymobTransactionId { get; private set; } = null!;
    public string? Issuer { get; private set; }
    public decimal? Amount { get; private set; }
    public string? StatusCode { get; private set; }
    public string? DisbursementStatus { get; private set; }
    public string RawPayload { get; private set; } = string.Empty;

    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    private PayoutWebhookEvent() { }

    public static PayoutWebhookEvent Create(
        string paymobTransactionId,
        string? issuer,
        decimal? amount,
        string? statusCode,
        string? disbursementStatus,
        string rawPayload) => new()
    {
        PaymobTransactionId = paymobTransactionId,
        Issuer = issuer,
        Amount = amount,
        StatusCode = statusCode,
        DisbursementStatus = disbursementStatus,
        RawPayload = rawPayload,
        ReceivedAt = AppNow
    };

    public bool IsProcessed => ProcessedAt is not null;

    public void MarkProcessed() => ProcessedAt = AppNow;

    public void MarkFailed(string error) => Error = error;
}
