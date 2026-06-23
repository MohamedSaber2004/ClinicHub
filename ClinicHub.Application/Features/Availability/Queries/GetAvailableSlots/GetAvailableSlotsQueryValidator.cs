using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryValidator: AbstractValidator<GetAvailableSlotsQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetAvailableSlotsQueryValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.ClinicId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x).CustomAsync(async (query, context, cancellationToken) =>
            {
                if (query.DoctorId == default || query.ClinicId == default)
                    return;

                var exists = await _ctx.DoctorRepository
                    .ExistsAsync(d => d.Id == query.DoctorId && d.ClinicId == query.ClinicId, cancellationToken);

                if (!exists)
                {
                    context.AddFailure("DoctorId", localizer[LocalizationKeys.Slots.DoctorIsNotFollowForThatClinic.Value]);
                }
            });
        }
    }
}
