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

            RuleFor(x => x.Dto)
                .NotNull().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MaximumLength(200).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Email)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .EmailAddress().WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(BeUniqueEmail).WithMessage(_localizer[LocalizationKeys.ClinicMessages.EmailAlreadyExists.Value]);

            RuleFor(x => x.Dto.SpecializationId)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.Dto.Phone)
                .MaximumLength(11).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$").WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(BeUniquePhone).WithMessage(_localizer[LocalizationKeys.ClinicMessages.PhoneAlreadyExists.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Dto.Phone));

            RuleFor(x => x.Dto.Website)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Logo)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.NameAr)
                .MaximumLength(200).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Description)
                .MaximumLength(1000).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.ArDescription)
                .MaximumLength(1000).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.Address)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.AddressAr)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.WorkingHours)
                .MaximumLength(500).WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);

            RuleFor(x => x.Dto.WorkingHoursStart)
                .LessThan(x => x.Dto.WorkingHoursEnd)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidTimeRange.Value])
                .When(x => x.Dto.WorkingHoursStart.HasValue && x.Dto.WorkingHoursEnd.HasValue);

            RuleFor(x => x.Dto.WorkingDays)
                .Must(BeValidDayNames)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidWorkingDays.Value])
                .When(x => x.Dto.WorkingDays != null && x.Dto.WorkingDays.Count > 0);
        }

        private static bool BeValidDayNames(List<DayOfWeek>? days)
        {
            if (days == null || days.Count == 0) return true;
            return days.All(d => Enum.IsDefined(typeof(DayOfWeek), d));
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
