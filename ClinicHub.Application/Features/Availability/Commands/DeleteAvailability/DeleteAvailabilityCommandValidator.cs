using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Commands.DeleteAvailability
{
    public class DeleteAvailabilityCommandValidator : AbstractValidator<DeleteAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public DeleteAvailabilityCommandValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(AvailabilityExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AvailabilityMessages.NotFound.Value]));
        }

        private async Task<bool> AvailabilityExists(Guid id, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorAvailabilityRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        }
    }
}
