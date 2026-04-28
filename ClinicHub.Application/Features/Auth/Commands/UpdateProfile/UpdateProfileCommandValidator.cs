using ClinicHub.Application.Localization;
using FluentValidation;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MaximumLength(200).WithMessage(string.Format(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value), 200));

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MaximumLength(50).WithMessage(string.Format(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value), 50));

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .LessThan(DateTime.UtcNow).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));
        }
    }
}
