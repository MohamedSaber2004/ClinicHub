using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorDetailsForMobile
{
    public class GetDoctorDetailsForMobileQueryValidator : AbstractValidator<GetDoctorDetailsForMobileQuery>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDoctorDetailsForMobileQueryValidator(IStringLocalizer<Messages> localizer, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.DoctorId)
                .NotEmpty()
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(DoctorExistsAsync)
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.DoctorMessages.NotFound.Value]));
        }

        private async Task<bool> DoctorExistsAsync(Guid doctorId, CancellationToken cancellationToken)
            => await _unitOfWork.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
    }
}
