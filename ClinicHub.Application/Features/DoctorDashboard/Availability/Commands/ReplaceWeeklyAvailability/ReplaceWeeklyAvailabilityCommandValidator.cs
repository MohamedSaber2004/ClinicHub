using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.ReplaceWeeklyAvailability
{
    public class ReplaceWeeklyAvailabilityCommandValidator : AbstractValidator<ReplaceWeeklyAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly ICurrentUserService _currentUser;

        public ReplaceWeeklyAvailabilityCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx, ICurrentUserService currentUser)
        {
            _ctx = ctx;
            _currentUser = currentUser;

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

            RuleFor(x => x.Days)
                .MustAsync(AllDaysWithinClinicSchedule)
                .WithMessage(localizer[LocalizationKeys.BookingMessages.ClinicClosed.Value])
                .When(x => x.Days != null && x.Days.Count > 0);
        }

        private async Task<bool> AllDaysWithinClinicSchedule(List<AvailabilityDayInput> days, CancellationToken cancellationToken)
        {
            var doctor = await _ctx.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUser.UserId && !d.IsDeleted, cancellationToken);
            if (doctor?.ClinicId is null)
                return true; // Handled by the handler (doctor must be assigned to a clinic).

            var clinic = await _ctx.ClinicRepository.GetByIdAsync(doctor.ClinicId.Value);
            return days.All(d => ClinicScheduleGuard.IsWithinClinicSchedule(clinic, d.DayOfWeek, d.StartTime, d.EndTime));
        }
    }
}
