using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
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
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(DoctorExists).WithMessage(localizer[LocalizationKeys.AppointmentMessages.DoctorNotFound.Value]);

            RuleFor(v => v.ClinicId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(ClinicExists).WithMessage(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]);

            RuleFor(v => v.AppointmentDate)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidDate.Value]);

            RuleFor(v => v.StartTime)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(v => v.EndTime)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .GreaterThan(v => v.StartTime).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]);

            RuleFor(v => v.PatientFullName)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(v => v.PatientPhoneNumber)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(20).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(v => v.PatientAge)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidAge.Value]);

            RuleFor(v => v.Complaint)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(v => v)
                .MustAsync(async (v, ct) => await DoctorIsAvailable(v.DoctorId, v.AppointmentDate, v.StartTime, v.EndTime, ct))
                .WithName("Appointment")
                .WithMessage(localizer[LocalizationKeys.AppointmentMessages.DoctorNotAvailableAtThisTime.Value])
                .MustAsync(async (v, ct) => !await HasOverlappingAppointment(v.DoctorId, v.AppointmentDate, v.StartTime, v.EndTime, ct))
                .WithName("Appointment")
                .WithMessage(localizer[LocalizationKeys.AppointmentMessages.TimeSlotAlreadyBooked.Value]);
        }

        private async Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }

        private Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }

        private Task<bool> DoctorIsAvailable(Guid doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
        {
            var dayOfWeek = appointmentDate.DayOfWeek;
            return _ctx.DoctorAvailabilityRepository.ExistsAsync(a =>
                a.DoctorId == doctorId &&
                a.DayOfWeek == dayOfWeek &&
                a.StartTime <= startTime &&
                a.EndTime >= endTime, cancellationToken);
        }

        private Task<bool> HasOverlappingAppointment(Guid doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken)
        {
            return _ctx.AppointmentRepository.HasOverlappingAppointmentAsync(doctorId, appointmentDate, startTime, endTime);
        }
    }
}
