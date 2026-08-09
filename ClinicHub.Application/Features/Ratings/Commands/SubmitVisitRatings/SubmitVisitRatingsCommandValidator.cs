using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ratings.Commands.SubmitVisitRatings
{
    public class SubmitVisitRatingsCommandValidator : AbstractValidator<SubmitVisitRatingsCommand>
    {
        public SubmitVisitRatingsCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(v => v.ClinicValue)
                .InclusiveBetween(1, 5).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.InvalidValue.Value]));

            RuleFor(v => v.CleanlinessValue)
                .InclusiveBetween(1, 5).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.InvalidValue.Value]));

            RuleFor(v => v.ReceptionValue)
                .InclusiveBetween(1, 5).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.InvalidValue.Value]));

            RuleFor(v => v.DoctorValue)
                .InclusiveBetween(1, 5).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.InvalidValue.Value]))
                .When(v => v.DoctorValue.HasValue);

            RuleFor(v => v)
                .Must(v => v.DoctorId != null || v.ClinicId != null)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.TargetRequired.Value]));

            RuleFor(v => v)
                .Must(v => v.DoctorId == null || v.DoctorValue.HasValue)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.DoctorValueRequired.Value]));

            RuleFor(v => v)
                .Must(v => v.DoctorValue == null || v.DoctorId != null)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.DoctorTargetRequired.Value]));

            RuleFor(v => v.Review)
                .MaximumLength(1000).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }
    }
}
