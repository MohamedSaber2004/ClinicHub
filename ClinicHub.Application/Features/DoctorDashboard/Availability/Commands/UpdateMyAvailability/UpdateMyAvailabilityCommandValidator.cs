using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.UpdateMyAvailability
{
    public class UpdateMyAvailabilityCommandValidator : AbstractValidator<UpdateMyAvailabilityCommand>
    {
        public UpdateMyAvailabilityCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value])
                .When(x => x.DayOfWeek.HasValue);

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value])
                .When(x => x.StartTime.HasValue && x.EndTime.HasValue);

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value])
                .LessThanOrEqualTo(480).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .When(x => x.SlotDurationMinutes.HasValue);
        }
    }
}
