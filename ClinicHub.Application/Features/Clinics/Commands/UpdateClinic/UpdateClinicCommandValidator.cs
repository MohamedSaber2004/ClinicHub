using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinic
{
    public class UpdateClinicCommandValidator : AbstractValidator<UpdateClinicCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateClinicCommandValidator(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(200).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .EmailAddress().WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(BeUniqueEmail).WithMessage(_localizer[LocalizationKeys.ClinicMessages.EmailAlreadyExists.Value]);

            RuleFor(x => x.SpecializationId)
                .MustAsync(BeExistingSpecialization)
                .WithMessage(_localizer[LocalizationKeys.SpecializationMessages.NotFound.Value])
                .When(x => x.SpecializationId.HasValue);

            RuleFor(x => x.Phone)
                .MaximumLength(11).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(BeUniquePhone).WithMessage(_localizer[LocalizationKeys.ClinicMessages.PhoneAlreadyExists.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.Website)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Logo)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.NameAr)
                .MaximumLength(200).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.ArDescription)
                .MaximumLength(1000).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.AddressAr)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.WorkingHours)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.WorkingHoursStart)
                .LessThan(x => x.WorkingHoursEnd)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value])
                .When(x => x.WorkingHoursStart.HasValue && x.WorkingHoursEnd.HasValue);

            RuleFor(x => x.WorkingDays)
                .Must(BeValidDayNames)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidWorkingDays.Value])
                .When(x => x.WorkingDays != null && x.WorkingDays.Count > 0);

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

        private static bool HaveBothCoordinates(UpdateClinicCommand command)
        {
            return command.Latitude.HasValue && command.Longitude.HasValue;
        }

        private static bool BeValidDayNames(List<DayOfWeek>? days)
        {
            if (days == null || days.Count == 0) return true;
            return days.All(d => Enum.IsDefined(typeof(DayOfWeek), d));
        }

        private async Task<bool> BeExistingSpecialization(UpdateClinicCommand command, Guid? specializationId, CancellationToken cancellationToken)
        {
            return specializationId.HasValue &&
                   await _unitOfWork.SpecializationRepository.ExistsByKeyAsync(specializationId.Value, cancellationToken);
        }

        private async Task<bool> BeUniqueEmail(UpdateClinicCommand command, string email, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Email == email && c.Id != command.Id, cancellationToken);
        }

        private async Task<bool> BeUniquePhone(UpdateClinicCommand command, string? phone, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Phone == phone && c.Id != command.Id, cancellationToken);
        }
    }
}
