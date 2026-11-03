using MediatR;

namespace ClinicHub.Application.Features.PayoutWebhook.Commands.ProcessPayoutWebhook;

/// <summary>
/// One Paymob payout callback. Stored verbatim and then re-inquired: the callback has no
/// signature to verify, so it is treated as a notification that something changed, never as
/// proof of it.
/// </summary>
public class ProcessPayoutWebhookCommand : IRequest<bool>
{
    public string TransactionId { get; set; } = string.Empty;
    public string? Issuer { get; set; }
    public decimal? Amount { get; set; }
    public string? StatusCode { get; set; }
    public string? DisbursementStatus { get; set; }
    public string RawPayload { get; set; } = string.Empty;
}
