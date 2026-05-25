using FluentValidation;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookCommandValidator : AbstractValidator<ConfirmPaymentWebhookCommand>
{
    public ConfirmPaymentWebhookCommandValidator()
    {
        RuleFor(x => x.Hmac)
            .NotEmpty();

        RuleFor(x => x.TransactionData)
            .NotEmpty();
    }
}
