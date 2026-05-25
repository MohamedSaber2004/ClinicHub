using System.Collections.Generic;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookRequestDto
{
    public string Hmac { get; set; } = string.Empty;
    public Dictionary<string, string> TransactionData { get; set; } = new Dictionary<string, string>();
}
