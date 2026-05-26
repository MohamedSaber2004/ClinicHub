using MediatR;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookCommand : IRequest<bool>
{
    public string Hmac { get; set; } = string.Empty;
    public PaymobTransaction Transaction { get; set; } = new();
}
