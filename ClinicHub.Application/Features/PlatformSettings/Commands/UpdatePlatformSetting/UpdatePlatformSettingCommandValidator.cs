using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.PlatformSettings.Commands.UpdatePlatformSetting
{
    public class UpdatePlatformSettingCommandValidator : AbstractValidator<UpdatePlatformSettingCommand>
    {
        public UpdatePlatformSettingCommandValidator(
            IStringLocalizer<Messages> localizer,
            IOptions<PayoutSettings> payoutSettings)
        {
            var floor = payoutSettings.Value.MinimumTransactionAmount;

            RuleFor(v => v.AppointmentFeePercent)
                .InclusiveBetween(0m, 100m)
                .When(v => v.AppointmentFeePercent.HasValue)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PayoutMessages.InvalidFeePercent.Value]));

            // Paymob refuses any transfer below its own floor, so a lower minimum would only
            // produce batches that fail at execution time.
            RuleFor(v => v.MinimumPayoutAmount)
                .GreaterThanOrEqualTo(floor)
                .When(v => v.MinimumPayoutAmount.HasValue)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(
                    localizer[LocalizationKeys.PayoutMessages.MinimumBelowPaymobFloor.Value].Value,
                    null,
                    floor));
        }
    }
}
