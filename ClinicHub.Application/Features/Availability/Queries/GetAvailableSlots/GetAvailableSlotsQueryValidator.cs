using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryValidator : AbstractValidator<GetAvailableSlotsQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetAvailableSlotsQueryValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.DoctorId)
                .NotEmpty();

            RuleFor(x => x.ClinicId)
                .NotEmpty();

            RuleFor(x => x)
                .MustAsync(async (v, ct) => await IsWithinBookingWindow(v.ClinicId, v.Date, ct))
                .WithName("Date")
                .WithMessage(localizer[LocalizationKeys.BookingMessages.InvalidDate])
                .When(x => x.Date.HasValue);
        }

        private async Task<bool> IsWithinBookingWindow(Guid clinicId, DateTime? date, CancellationToken cancellationToken)
        {
            if (!date.HasValue) return true;

            var config = await _ctx.BookingConfigurationRepository.GetByClinicIdAsync(clinicId);
            return config == null || date.Value.Date <= DateTime.Now.Date.AddDays(config.MaxAdvanceBookingDays);
        }
    }
}
