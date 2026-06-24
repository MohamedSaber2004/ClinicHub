using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.DeactivateClinic
{
    public class DeactivateClinicCommandValidator: AbstractValidator<DeactivateClinicCommand>
    {
        private readonly IUnitOfWork _ctx;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeactivateClinicCommandValidator(IUnitOfWork ctx, IStringLocalizer<Messages> localizer)
        {
            _ctx = ctx;
            this._localizer = localizer;
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .NotNull().WithMessage(_localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(ClinicIdExists).WithMessage(_localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]);
        }

        private async Task<bool> ClinicIdExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }
    }
}
