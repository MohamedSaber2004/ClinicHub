using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Queries.GetPaginatedClinics
{
    public class GetPaginatedClinicsQueryValidator : AbstractValidator<GetPaginatedClinicsQuery>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetPaginatedClinicsQueryValidator(IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeGreaterThanOrEqualToOne.Value])
                .LessThanOrEqualTo(100)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]);

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value])
                .When(x => x.Status.HasValue);

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value])
                .MustAsync(ClinicEmailExists)
                .WithMessage(_localizer[LocalizationKeys.ClinicMessages.EmailNotFound.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Name)
                .MaximumLength(200)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Phone)
                .MaximumLength(11)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .Matches(@"^01[0125][0-9]{8}$")
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidFormat.Value])
                .MustAsync(ClinicPhoneExists)
                .WithMessage(_localizer[LocalizationKeys.ClinicMessages.PhoneNotFound.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.SearchTerm)
                .MaximumLength(200)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.MaxLength.Value])
                .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

            RuleFor(x => x)
                .Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue || x.CreatedFrom.Value <= x.CreatedTo.Value)
                .WithMessage(_localizer[LocalizationKeys.ValidationMessages.InvalidDateRange.Value])
                .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue);
        }

        private async Task<bool> ClinicEmailExists(string email, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Email == email, cancellationToken);
        }

        private async Task<bool> ClinicPhoneExists(string phone, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Phone == phone, cancellationToken);
        }
    }
}
