using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Commands.CreateBookingConfig
{
    public class CreateBookingConfigCommandValidator : AbstractValidator<CreateBookingConfigCommand>
    {
        private readonly IUnitOfWork _ctx;

        public CreateBookingConfigCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.ClinicId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value])
                .MustAsync(async(clinicId, cancellationToken) => await ClinicExists(clinicId, cancellationToken))
                .WithMessage(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]);

            RuleFor(x => x.Dto)
                .NotNull().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.Dto.ConsultationFee)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            RuleFor(x => x.Dto.MaxAdvanceBookingDays)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

RuleFor(x => x.Dto.ReservationTtlMinutes)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);

            RuleFor(x => x.Dto.CancellationWindowMinutes)
                .GreaterThan(0).WithMessage(localizer[LocalizationKeys.ValidationMessages.MustBeGreaterThanZero.Value]);
        }

        private async Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }
    }
}
