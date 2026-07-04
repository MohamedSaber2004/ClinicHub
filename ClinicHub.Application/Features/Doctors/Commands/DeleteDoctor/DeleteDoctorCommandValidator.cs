using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Doctors.Commands.DeleteDoctor
{
    public class DeleteDoctorCommandValidator : AbstractValidator<DeleteDoctorCommand>
    {
        private readonly IUnitOfWork _ctx;

        public DeleteDoctorCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.DoctorMessages.NotFound.Value]));
        }

        private async Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }
    }
}
