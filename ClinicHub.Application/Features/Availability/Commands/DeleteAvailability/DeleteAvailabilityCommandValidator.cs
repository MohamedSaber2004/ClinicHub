using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;

namespace ClinicHub.Application.Features.Availability.Commands.DeleteAvailability
{
    public class DeleteAvailabilityCommandValidator : AbstractValidator<DeleteAvailabilityCommand>
    {
        private readonly IUnitOfWork _ctx;

        public DeleteAvailabilityCommandValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(AvailabilityExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AvailabilityMessages.NotFound.Value));
        }

        private async Task<bool> AvailabilityExists(Guid id, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorAvailabilityRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        }
    }
}
