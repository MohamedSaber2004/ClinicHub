using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.UpdateMyAvailability
{
    public class UpdateMyAvailabilityCommandValidator : AbstractValidator<UpdateMyAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateMyAvailabilityCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

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

            RuleFor(x => x)
                .MustAsync(WithinClinicSchedule)
                .WithName("Availability")
                .WithMessage(localizer[LocalizationKeys.BookingMessages.ClinicClosed.Value]);
        }

        private async Task<bool> WithinClinicSchedule(UpdateMyAvailabilityCommand command, CancellationToken cancellationToken)
        {
            if (!command.DayOfWeek.HasValue && !command.StartTime.HasValue && !command.EndTime.HasValue)
                return true; // Nothing schedule-related changes.

            var availability = await _ctx.DoctorAvailabilityRepository.GetByIdAsync(command.Id);
            if (availability is null)
                return true; // Reported by the handler (NotFound).

            var clinic = await _ctx.ClinicRepository.GetByIdAsync(availability.ClinicId);
            return ClinicScheduleGuard.IsWithinClinicSchedule(
                clinic,
                command.DayOfWeek ?? availability.DayOfWeek,
                command.StartTime ?? availability.StartTime,
                command.EndTime ?? availability.EndTime);
        }
    }
}
