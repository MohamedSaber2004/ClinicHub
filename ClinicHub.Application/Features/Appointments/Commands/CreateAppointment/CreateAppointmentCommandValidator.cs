using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateAppointmentCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.DoctorNotFound.Value]));

            RuleFor(v => v.ClinicId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(ClinicExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]))
                .MustAsync(HasBookingConfiguration).WithMessage(localizer[LocalizationKeys.BookingMessages.BookingConfigNotFound]);

            RuleFor(v => v.AppointmentDate)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidDate.Value]));

            RuleFor(v => v.StartTime)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(v => v.EndTime)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .GreaterThan(v => v.StartTime).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]));

            RuleFor(v => v.PatientFullName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(200).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(v => v.PatientPhoneNumber)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MaximumLength(20).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(v => v.PatientAge)
                .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidAge.Value]));

            RuleFor(v => v.Complaint)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(v => v)
                .MustAsync(async (v, ct) => await IsWithinBookingWindow(v.ClinicId, v.AppointmentDate, ct))
                .WithName("AppointmentDate")
                .WithMessage(localizer[LocalizationKeys.BookingMessages.InvalidDate])
                .MustAsync(async (v, ct) => await DoctorIsAvailable(v.DoctorId, v.ClinicId, v.AppointmentDate, v.StartTime, v.EndTime, ct))
                .WithName("Appointment")
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.DoctorNotAvailableAtThisTime.Value]))
                .MustAsync(async (v, ct) => !await HasOverlappingAppointment(v.DoctorId, v.AppointmentDate, v.StartTime, v.EndTime, ct))
                .WithName("Appointment")
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.TimeSlotAlreadyBooked.Value]));
        }

        private async Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }

        private Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }

        private async Task<bool> HasBookingConfiguration(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.BookingConfigurationRepository.GetByClinicIdAsync(clinicId) != null;
        }

        private async Task<bool> IsWithinBookingWindow(Guid clinicId, DateTime appointmentDate, CancellationToken cancellationToken)
        {
            var config = await _ctx.BookingConfigurationRepository.GetByClinicIdAsync(clinicId);
            return config == null || appointmentDate.Date <= DateTime.UtcNow.Date.AddDays(config.MaxAdvanceBookingDays);
        }

        private async Task<bool> DoctorIsAvailable(Guid doctorId, Guid clinicId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
        {
            var dayOfWeek = appointmentDate.DayOfWeek;
            var durationMinutes = (endTime - startTime).TotalMinutes;

            var availabilities = await _ctx.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == doctorId && a.ClinicId == clinicId && a.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            return availabilities.Any(a =>
                a.StartTime <= startTime &&
                a.EndTime >= endTime &&
                durationMinutes >= a.SlotDurationMinutes &&
                durationMinutes % a.SlotDurationMinutes == 0 &&
                IsAlignedToSlot(a.StartTime, startTime, a.SlotDurationMinutes));
        }

        private static bool IsAlignedToSlot(TimeSpan availabilityStart, TimeSpan slotStart, int slotDurationMinutes)
        {
            var minutesSinceStart = (slotStart - availabilityStart).TotalMinutes;
            return minutesSinceStart >= 0 && minutesSinceStart % slotDurationMinutes == 0;
        }

        private Task<bool> HasOverlappingAppointment(Guid doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
        {
            return _ctx.AppointmentRepository.HasOverlappingAppointmentAsync(doctorId, appointmentDate, startTime, endTime);
        }
    }
}
