using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.ActivateClinic
{
    public class ActivateClinicCommandValidator : AbstractValidator<ActivateClinicCommand>
    {
        private readonly IUnitOfWork _ctx;

        public ActivateClinicCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .NotNull().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(ClinicExists).WithMessage(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]);
            _ctx = ctx;
        }

        private async Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository
                .GetAllAsync(null)
                .IgnoreQueryFilters()
                .AnyAsync(c => c.Id == clinicId, cancellationToken);
        }
    }
}
