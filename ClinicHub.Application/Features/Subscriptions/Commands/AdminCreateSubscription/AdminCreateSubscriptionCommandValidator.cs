using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Subscriptions.Commands.AdminCreateSubscription
{
    public class AdminCreateSubscriptionCommandValidator : AbstractValidator<AdminCreateSubscriptionCommand>
    {
        public AdminCreateSubscriptionCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(v => v.ClinicId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(v => v.PlanId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(v => v.Period)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

            RuleFor(v => v.StartDate)
                .Must(date => !date.HasValue || date.Value.Date >= DateTime.Now.Date)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.SubscriptionMessages.PastStartDate.Value]));

            RuleFor(v => v.Amount)
                .Must(amount => !amount.HasValue || amount.Value > 0)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));
        }
    }
}
