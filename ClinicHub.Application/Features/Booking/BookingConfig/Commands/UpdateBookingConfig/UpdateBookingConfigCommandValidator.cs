using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Booking.BookingConfig.Commands.UpdateBookingConfig
{
    public class UpdateBookingConfigCommandValidator : AbstractValidator<UpdateBookingConfigCommand>
    {
        public UpdateBookingConfigCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.ClinicId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

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
    }
}
