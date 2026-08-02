using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public class DeleteAdPackageCommandValidator : AbstractValidator<DeleteAdPackageCommand>
{
    public DeleteAdPackageCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);
    }
}
