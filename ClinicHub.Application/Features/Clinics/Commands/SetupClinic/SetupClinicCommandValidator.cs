using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Commands.SetupClinic
{
    public class SetupClinicCommandValidator : AbstractValidator<SetupClinicCommand>
    {
        private readonly IUnitOfWork _ctx;

        public SetupClinicCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .MaximumLength(200).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength]);

            RuleFor(x => x.Dto.SpecializationId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required])
                .MustAsync(SpecializationExists).WithMessage(localizer[LocalizationKeys.SpecializationMessages.NotFound]);

            RuleFor(x => x.Dto.Lat)
                .InclusiveBetween(-90, 90).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat]);

            RuleFor(x => x.Dto.Lng)
                .InclusiveBetween(-180, 180).WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidFormat]);
        }

        private async Task<bool> SpecializationExists(Guid specializationId, CancellationToken cancellationToken)
        {
            return await _ctx.SpecializationRepository.ExistsAsync(s => s.Id == specializationId, cancellationToken);
        }
    }
}
