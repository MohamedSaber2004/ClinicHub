using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Commands.CreateNewAvailability
{
    public class CreateNewAvailabilityCommandValidator : AbstractValidator<CreateNewAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateNewAvailabilityCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .MustAsync(DoctorExists).WithMessage(localizer[LocalizationKeys.AppointmentMessages.DoctorNotFound]);

            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue]);

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required]);

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .GreaterThan(x => x.StartTime).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange]);

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat])
                .LessThanOrEqualTo(480).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat]);
        }

        private async Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }
    }
}
