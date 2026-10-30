using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.CreateMyAvailability
{
    public class CreateMyAvailabilityCommandValidator : AbstractValidator<CreateMyAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly ICurrentUserService _currentUser;

        public CreateMyAvailabilityCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx, ICurrentUserService currentUser)
        {
            _ctx = ctx;
            _currentUser = currentUser;

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

            RuleFor(x => x)
                .MustAsync(WithinClinicSchedule)
                .WithName("Availability")
                .WithMessage(localizer[LocalizationKeys.BookingMessages.ClinicClosed.Value]);
        }

        private async Task<bool> WithinClinicSchedule(CreateMyAvailabilityCommand command, CancellationToken cancellationToken)
        {
            var doctor = await _ctx.DoctorRepository.GetFirstAsync(
                d => d.UserId == _currentUser.UserId && !d.IsDeleted, cancellationToken);
            if (doctor?.ClinicId is null)
                return true; // Handled by the handler (doctor must be assigned to a clinic).

            var clinic = await _ctx.ClinicRepository.GetByIdAsync(doctor.ClinicId.Value);
            return ClinicScheduleGuard.IsWithinClinicSchedule(clinic, command.DayOfWeek, command.StartTime, command.EndTime);
        }
    }
}
