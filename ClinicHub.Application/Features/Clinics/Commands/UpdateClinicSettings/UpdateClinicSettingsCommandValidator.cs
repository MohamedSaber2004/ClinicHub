using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinicSettings
{
    public class UpdateClinicSettingsCommandValidator : AbstractValidator<UpdateClinicSettingsCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateClinicSettingsCommandValidator(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(200).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.SpecializationId)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(BeExistingSpecialization).WithMessage(_localizer[LocalizationKeys.SpecializationMessages.NotFound.Value]);

            RuleFor(x => x.Phone)
                .MaximumLength(11).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.ResponsibleDoctor)
                .MaximumLength(200).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.ManagerName)
                .MaximumLength(200).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Location)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.ConsultationFee)
                .GreaterThan(0).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            RuleFor(x => x.MaxAdvanceBookingDays)
                .GreaterThan(0).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            RuleFor(x => x.ReservationTtlMinutes)
                .GreaterThan(0).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            RuleFor(x => x.CancellationWindowMinutes)
                .GreaterThan(0).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            RuleFor(x => x.Currency)
                .MaximumLength(3).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Currency));

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage(_localizer[LocalizationKeys.ClinicMessages.InvalidLatitude.Value])
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage(_localizer[LocalizationKeys.ClinicMessages.InvalidLongitude.Value])
                .When(x => x.Longitude.HasValue);

            RuleFor(x => x)
                .Must(HaveBothCoordinates)
                .WithMessage(_localizer[LocalizationKeys.ClinicMessages.InvalidCoordinates.Value])
                .When(x => x.Latitude.HasValue || x.Longitude.HasValue);
        }

        private static bool HaveBothCoordinates(UpdateClinicSettingsCommand command)
        {
            return command.Latitude.HasValue && command.Longitude.HasValue;
        }

        private async Task<bool> BeExistingSpecialization(UpdateClinicSettingsCommand command, Guid specializationId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.SpecializationRepository.ExistsByKeyAsync(specializationId, cancellationToken);
        }
    }
}
