using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.CreateMyAvailability
{
    public class CreateMyAvailabilityCommandValidator : AbstractValidator<CreateMyAvailabilityCommand>
    {
        public CreateMyAvailabilityCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]);

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .GreaterThan(x => x.StartTime).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]);

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value])
                .LessThanOrEqualTo(480).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]);
        }
    }
}
