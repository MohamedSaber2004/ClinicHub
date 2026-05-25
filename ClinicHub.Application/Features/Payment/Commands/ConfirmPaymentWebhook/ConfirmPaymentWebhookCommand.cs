using MediatR;
using System.Collections.Generic;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookCommand : IRequest<bool>
{
    public string Hmac { get; set; } = string.Empty;
    public IDictionary<string, string> TransactionData { get; set; } = new Dictionary<string, string>();
}
