using FluentValidation;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookCommandValidator : AbstractValidator<ConfirmPaymentWebhookCommand>
{
    public ConfirmPaymentWebhookCommandValidator()
    {
        RuleFor(x => x.Hmac)
            .NotEmpty()
            .WithMessage("HMAC must not be empty");

        RuleFor(x => x.Transaction)
            .NotNull()
            .WithMessage("Transaction must not be null");

        RuleFor(x => x.Transaction.Order.Id)
            .GreaterThan(0)
            .WithMessage("Order ID must be greater than 0");
    }
}
