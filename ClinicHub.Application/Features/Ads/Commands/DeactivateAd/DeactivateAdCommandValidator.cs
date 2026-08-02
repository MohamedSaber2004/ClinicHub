using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.DeactivateAd;

public class DeactivateAdCommandValidator : AbstractValidator<DeactivateAdCommand>
{
    public DeactivateAdCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

        RuleFor(v => v.Reason)
            .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);
    }
}
