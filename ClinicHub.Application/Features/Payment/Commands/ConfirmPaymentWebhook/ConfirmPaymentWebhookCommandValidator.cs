using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Payment.Commands.ConfirmPaymentWebhook;

public class ConfirmPaymentWebhookCommandValidator : AbstractValidator<ConfirmPaymentWebhookCommand>
{
    public ConfirmPaymentWebhookCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Hmac)
            .NotEmpty()
            .WithMessage(localizer[LocalizationKeys.PaymentMessages.HmacRequired.Value]);

        RuleFor(x => x.Transaction)
            .NotNull()
            .WithMessage(localizer[LocalizationKeys.PaymentMessages.TransactionRequired.Value]);

        RuleFor(x => x.Transaction.Order.Id)
            .GreaterThan(0)
            .WithMessage(localizer[LocalizationKeys.PaymentMessages.InvalidOrderId.Value]);
    }
}
