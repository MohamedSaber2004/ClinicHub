using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ratings.Commands.CreateRating
{
    public class CreateRatingCommandValidator : AbstractValidator<CreateRatingCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateRatingCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v)
                .Must(v => v.DoctorId != null || v.ClinicId != null)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.TargetRequired.Value]));

            RuleFor(v => v)
                .Must(v => v.DoctorId == null || v.ClinicId == null)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.SingleTargetRequired.Value]));

            RuleFor(v => v.Value)
                .InclusiveBetween(1, 5).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RatingMessages.InvalidValue.Value]));

            RuleFor(v => v.Review)
                .MaximumLength(1000).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }
    }
}
