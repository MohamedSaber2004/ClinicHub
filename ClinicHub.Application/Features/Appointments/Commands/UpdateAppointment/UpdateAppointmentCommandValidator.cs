using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Appointments.Commands.UpdateAppointment
{
    public class UpdateAppointmentCommandValidator : AbstractValidator<UpdateAppointmentCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateAppointmentCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.AppointmentId)
                .MustAsync(AppointmentExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.AppointmentNotFound.Value]));

            RuleFor(v => v.Dto.AppointmentDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidDate.Value]))
                .When(v => v.Dto.AppointmentDate.HasValue);

            RuleFor(v => v.Dto.EndTime)
                .GreaterThan(v => v.Dto.StartTime).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]))
                .When(v => v.Dto.StartTime.HasValue && v.Dto.EndTime.HasValue);

            RuleFor(v => v)
                .MustAsync(async (v, ct) => await DoctorIsAvailable(v, ct))
                .WithName("Appointment")
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.DoctorNotAvailableAtThisTime.Value]))
                .MustAsync(async (v, ct) => !await HasOverlappingAppointment(v, ct))
                .WithName("Appointment")
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.TimeSlotAlreadyBooked.Value]));
        }

        private async Task<bool> AppointmentExists(Guid appointmentId, CancellationToken cancellationToken)
        {
            return await _ctx.AppointmentRepository.ExistsAsync(a => a.Id == appointmentId, cancellationToken);
        }

        private async Task<bool> DoctorIsAvailable(UpdateAppointmentCommand command, CancellationToken cancellationToken)
        {
            if (!command.Dto.AppointmentDate.HasValue || !command.Dto.StartTime.HasValue || !command.Dto.EndTime.HasValue) return true;
            
            var appointment = await _ctx.AppointmentRepository.GetByIdAsync(command.AppointmentId);
            if (appointment == null) return false;

            var dayOfWeek = command.Dto.AppointmentDate.Value.DayOfWeek;
            var startTime = command.Dto.StartTime.Value;
            var endTime = command.Dto.EndTime.Value;

            var availabilities = await _ctx.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == appointment.DoctorId && a.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            return availabilities.Any(a =>
                a.StartTime <= startTime &&
                a.EndTime >= endTime &&
                (endTime - startTime).TotalMinutes == a.SlotDurationMinutes &&
                IsAlignedToSlot(a.StartTime, startTime, a.SlotDurationMinutes));
        }

        private static bool IsAlignedToSlot(TimeSpan availabilityStart, TimeSpan slotStart, int slotDurationMinutes)
        {
            var minutesSinceStart = (slotStart - availabilityStart).TotalMinutes;
            return minutesSinceStart >= 0 && minutesSinceStart % slotDurationMinutes == 0;
        }

        private async Task<bool> HasOverlappingAppointment(UpdateAppointmentCommand command, CancellationToken cancellationToken)
        {
            if (!command.Dto.AppointmentDate.HasValue || !command.Dto.StartTime.HasValue || !command.Dto.EndTime.HasValue) return false;

            var appointment = await _ctx.AppointmentRepository.GetByIdAsync(command.AppointmentId);
            if (appointment == null) return false;

            var hasOverlap = await _ctx.AppointmentRepository.HasOverlappingAppointmentAsync(
                appointment.DoctorId, 
                command.Dto.AppointmentDate.Value, 
                command.Dto.StartTime.Value, 
                command.Dto.EndTime.Value);
            
            if (hasOverlap)
            {
                var dailyAppointments = await _ctx.AppointmentRepository.GetAppointmentsByDoctorAndDateAsync(appointment.DoctorId, command.Dto.AppointmentDate.Value);
                var actualOverlaps = dailyAppointments.Where(a => a.Id != command.AppointmentId && a.StartTime < command.Dto.EndTime.Value && a.EndTime > command.Dto.StartTime.Value);
                return actualOverlaps.Any();
            }

            return false;
        }
    }
}
