using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().When(x => x.FullName != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(200).When(x => x.FullName != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().When(x => x.PhoneNumber != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(11).When(x => x.PhoneNumber != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]))
                .Matches(@"^01[0125][0-9]{8}$").When(x => x.PhoneNumber != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));

            RuleFor(x => x.BirthDate)
                .NotEmpty().When(x => x.BirthDate != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .GreaterThan(DateOnly.FromDateTime(new DateTime(1900, 1, 1))).When(x => x.BirthDate != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]))
                .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).When(x => x.BirthDate != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));
            RuleFor(x => x.Gender)
                .IsInEnum().When(x => x.Gender != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));
            
            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(1000).When(x => x.ProfileImageUrl != null).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }
    }
}
