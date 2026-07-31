using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.ReplaceWeeklyAvailability
{
    public class ReplaceWeeklyAvailabilityCommandValidator : AbstractValidator<ReplaceWeeklyAvailabilityCommand>
    {
        public ReplaceWeeklyAvailabilityCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Days)
                .NotNull().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleForEach(x => x.Days).ChildRules(day =>
            {
                day.RuleFor(x => x.DayOfWeek)
                    .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]);

                day.RuleFor(x => x.StartTime)
                    .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

                day.RuleFor(x => x.EndTime)
                    .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                    .GreaterThan(x => x.StartTime).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]);

                day.RuleFor(x => x.SlotDurationMinutes)
                    .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value])
                    .LessThanOrEqualTo(480).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]);
            });
        }
    }
}
