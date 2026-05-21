using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryValidator: AbstractValidator<GetAvailableSlotsQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetAvailableSlotsQueryValidator(IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(DoctorExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.RealTimeMessages.ConversationNotFound.Value));

            RuleFor(x => x.ClinicId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.ValidationMessages.Required.Value))
                .MustAsync(ClinicExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(LocalizationKeys.AppointmentMessages.DoctorNotFound.Value));
        }

        private Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }

        private Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }   
    }
}
