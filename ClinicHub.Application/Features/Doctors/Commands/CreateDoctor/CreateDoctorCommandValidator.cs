using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Doctors.Commands.CreateDoctor
{
    public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateDoctorCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.ClinicId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(ClinicExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]));

            RuleFor(v => v.UserId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(v => v.SpecializationId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(SpecializationExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.SpecializationMessages.NotFound.Value]));

            RuleFor(v => v.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));
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
