using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ratings.Commands.CreateRating
{
    public class CreateRatingCommandValidator : AbstractValidator<CreateRatingCommand>
    {
        public CreateRatingCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(v => v.Type)
                .IsInEnum().When(v => v.Type.HasValue)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.InvalidValue.Value]));

            RuleFor(v => v)
                .Must(HasValidTarget)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.TargetRequired.Value]));

            RuleFor(v => v)
                .Must(v => v.DoctorId == null || v.ClinicId == null)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.SingleTargetRequired.Value]));

            RuleFor(v => v.Value)
                .InclusiveBetween(1, 5).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.InvalidValue.Value]));

            RuleFor(v => v.Review)
                .MaximumLength(1000).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }

        private static bool HasValidTarget(CreateRatingCommand v)
        {
            if (v.Type == RatingType.Doctor)
                return v.DoctorId != null && v.ClinicId == null;

            if (v.Type == RatingType.Clinic || v.Type == RatingType.PlaceCleanliness || v.Type == RatingType.Reception)
                return v.ClinicId != null && v.DoctorId == null;

            return v.DoctorId != null || v.ClinicId != null;
        }
    }
}
