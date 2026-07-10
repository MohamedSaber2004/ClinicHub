using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Specializations.Commands.CreateSpecialization
{
    public class CreateSpecializationCommandValidator : AbstractValidator<CreateSpecializationCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateSpecializationCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]))
                .MustAsync(BeUniqueName).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.SpecializationMessages.UniqueName.Value]));

            RuleFor(x => x.ArName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]))
                .MustAsync(BeUniqueArName).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.SpecializationMessages.UniqueArName.Value]));
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
            _ctx = ctx;
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            return !await _ctx.SpecializationRepository.ExistsAsync(s => s.Name == name, cancellationToken);
        }

        private async Task<bool> BeUniqueArName(string arName, CancellationToken cancellationToken)
        {
            return !await _ctx.SpecializationRepository.ExistsAsync(s => s.ArName == arName, cancellationToken);
        }
    }
}
