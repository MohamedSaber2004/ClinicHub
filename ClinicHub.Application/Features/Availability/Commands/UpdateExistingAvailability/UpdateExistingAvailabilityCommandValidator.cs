using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;

namespace ClinicHub.Application.Features.Availability.Commands.UpdateExistingAvailability
{
    public class UpdateExistingAvailabilityCommandValidator : AbstractValidator<UpdateExistingAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateExistingAvailabilityCommandValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(AvailabilityExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AvailabilityMessages.NotFound.Value));

            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidEnumValue.Value));

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidTimeRange.Value));

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value))
                .LessThanOrEqualTo(480).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.InvalidFormat.Value));
        }

        private async Task<bool> AvailabilityExists(Guid id, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorAvailabilityRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        }
    }
}
