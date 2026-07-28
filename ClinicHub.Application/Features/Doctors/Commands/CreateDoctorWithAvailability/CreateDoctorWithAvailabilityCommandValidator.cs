using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Doctors.Commands.CreateDoctorWithAvailability
{
    public class CreateDoctorWithAvailabilityCommandValidator : AbstractValidator<CreateDoctorWithAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateDoctorWithAvailabilityCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.ClinicId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .MustAsync(ClinicExists).WithMessage(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound]);

            RuleFor(v => v.UserId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required]);

            RuleFor(v => v.SpecializationId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .MustAsync(SpecializationExists).WithMessage(localizer[LocalizationKeys.SpecializationMessages.NotFound]);

            RuleFor(v => v.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero]);

            RuleForEach(v => v.Availabilities).ChildRules(av =>
            {
                av.RuleFor(x => x.DayOfWeek)
                    .IsInEnum().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue]);

                av.RuleFor(x => x.StartTime)
                    .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required]);

                av.RuleFor(x => x.EndTime)
                    .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                    .GreaterThan(x => x.StartTime).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange]);

                av.RuleFor(x => x.SlotDurationMinutes)
                    .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat])
                    .LessThanOrEqualTo(480).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat]);
            });
        }

        private async Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }

        private async Task<bool> SpecializationExists(Guid specializationId, CancellationToken cancellationToken)
        {
            return await _ctx.SpecializationRepository.ExistsAsync(s => s.Id == specializationId, cancellationToken);
        }
    }
}
