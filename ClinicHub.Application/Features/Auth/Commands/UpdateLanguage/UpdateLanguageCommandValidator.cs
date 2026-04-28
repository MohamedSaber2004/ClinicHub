using ClinicHub.Application.Localization;
using FluentValidation;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateLanguage
{
    public class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
    {
        public UpdateLanguageCommandValidator()
        {
            RuleFor(x => x.Language)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));
        }
    }
}
