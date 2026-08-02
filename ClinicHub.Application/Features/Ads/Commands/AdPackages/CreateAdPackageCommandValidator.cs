using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public class CreateAdPackageCommandValidator : AbstractValidator<CreateAdPackageCommand>
{
    public CreateAdPackageCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
            .MaximumLength(100).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

        RuleFor(v => v.NameAr).MaximumLength(100).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);
        RuleFor(v => v.Description).MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);
        RuleFor(v => v.DescriptionAr).MaximumLength(500).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

        RuleFor(v => v.DurationDays)
            .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);
    }
}
