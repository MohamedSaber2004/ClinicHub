using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Plans.Commands.CreatePlan
{
    public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
    {
        public CreatePlanCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(200).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(v => v.PriceMonthly)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to zero");

            RuleFor(v => v.PriceYearly)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to zero");
        }
    }
}
