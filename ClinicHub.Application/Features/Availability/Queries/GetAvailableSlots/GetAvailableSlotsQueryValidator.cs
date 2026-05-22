using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Availability.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryValidator: AbstractValidator<GetAvailableSlotsQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetAvailableSlotsQueryValidator(IStringLocalizer<Messages> localizer,IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.RealTimeMessages.ConversationNotFound.Value]));
        }

        private Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        } 
    }
}
