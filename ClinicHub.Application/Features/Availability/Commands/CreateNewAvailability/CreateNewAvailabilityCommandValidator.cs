using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Commands.CreateNewAvailability
{
    public class CreateNewAvailabilityCommandValidator : AbstractValidator<CreateNewAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateNewAvailabilityCommandValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[localizer[LocalizationKeys.ValidationMessages.Required.Value]]))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.DoctorNotFound.Value]));

            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .GreaterThan(x => x.StartTime).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value]));

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]))
                .LessThanOrEqualTo(480).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value]));
        }

        private async Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }
    }
}
