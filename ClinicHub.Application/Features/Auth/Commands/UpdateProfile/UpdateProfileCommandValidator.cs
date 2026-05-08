using ClinicHub.Application.Localization;
using FluentValidation;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().When(x => x.FullName != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MaximumLength(200).When(x => x.FullName != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value));

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().When(x => x.PhoneNumber != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MaximumLength(50).When(x => x.PhoneNumber != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value));

            RuleFor(x => x.BirthDate)
                .NotEmpty().When(x => x.BirthDate != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .LessThan(DateTime.UtcNow).When(x => x.BirthDate != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));

            RuleFor(x => x.Gender)
                .IsInEnum().When(x => x.Gender != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));
            
            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(1000).When(x => x.ProfileImageUrl != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.MaxLength.Value));
        }
    }
}
