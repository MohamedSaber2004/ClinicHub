using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
    {
        private readonly IUnitOfWork _ctx;

        public UpdateDoctorCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.DoctorMessages.NotFound.Value]));

            RuleFor(v => v.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]));
        }

        private async Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorRepository.GetAllAsync(null).IgnoreQueryFilters().AnyAsync(d => d.Id == doctorId, cancellationToken);
        }
    }
}
