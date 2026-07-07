using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Specializations.Commands.CreateSpecialization
{
    public class CreateSpecializationCommandValidator : AbstractValidator<CreateSpecializationCommand>
    {
        public CreateSpecializationCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(x => x.ArName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }
    }
}
