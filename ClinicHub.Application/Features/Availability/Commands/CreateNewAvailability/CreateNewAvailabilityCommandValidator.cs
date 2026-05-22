using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;

namespace ClinicHub.Application.Features.Availability.Commands.CreateNewAvailability
{
    public class CreateNewAvailabilityCommandValidator : AbstractValidator<CreateNewAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateNewAvailabilityCommandValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AppointmentMessages.DoctorNotFound.Value));

            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidEnumValue.Value));

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value));

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .GreaterThan(x => x.StartTime).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidTimeRange.Value));

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value))
                .LessThanOrEqualTo(480).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));
        }

        private async Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }
    }
}
