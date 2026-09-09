using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Commands.UpdateExistingAvailability
{
    public class UpdateExistingAvailabilityCommandValidator : AbstractValidator<UpdateExistingAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateExistingAvailabilityCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(AvailabilityExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AvailabilityMessages.NotFound.Value]));

            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]));

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]))
                .LessThanOrEqualTo(480).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));

            RuleFor(x => x)
                .MustAsync(WithinClinicSchedule)
                .WithName("Availability")
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.BookingMessages.ClinicClosed.Value]));
        }

        private async Task<bool> AvailabilityExists(Guid id, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorAvailabilityRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        }

        private async Task<bool> WithinClinicSchedule(UpdateExistingAvailabilityCommand command, CancellationToken cancellationToken)
        {
            if (!command.DayOfWeek.HasValue && !command.StartTime.HasValue && !command.EndTime.HasValue)
                return true; // Nothing schedule-related changes.

            var availability = await _ctx.DoctorAvailabilityRepository.GetByIdAsync(command.Id);
            if (availability is null)
                return true; // Reported by the AvailabilityExists rule.

            var clinic = await _ctx.ClinicRepository.GetByIdAsync(availability.ClinicId);
            return ClinicScheduleGuard.IsWithinClinicSchedule(
                clinic,
                command.DayOfWeek ?? availability.DayOfWeek,
                command.StartTime ?? availability.StartTime,
                command.EndTime ?? availability.EndTime);
        }
    }
}
