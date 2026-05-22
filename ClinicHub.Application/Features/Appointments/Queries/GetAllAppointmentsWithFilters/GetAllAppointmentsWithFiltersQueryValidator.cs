using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAllAppointmentsWithFilters
{
    public class GetAllAppointmentsWithFiltersQueryValidator: AbstractValidator<GetAllAppointmentsWithFiltersQuery>
    {
        private readonly IUnitOfWork _ctx;

        public GetAllAppointmentsWithFiltersQueryValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
            
            RuleFor(v => v.PageSize)
                .GreaterThan(0).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));

            RuleFor(v => v.EndDate)
                .GreaterThanOrEqualTo(v => v.StartDate).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidDate.Value]))
                .When(v => v.StartDate.HasValue && v.EndDate.HasValue);

            RuleFor(v => v.DoctorId)
                .MustAsync(async (doctorId, ct) => await DoctorExists(doctorId!.Value, ct))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.DoctorNotFound.Value]))
                .When(v => v.DoctorId.HasValue);

            RuleFor(v => v.ClinicId)
                .MustAsync(async (clinicId, ct) => await ClinicExists(clinicId!.Value, ct))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ClinicMessages.ClinicNotFound.Value]))
                .When(v => v.ClinicId.HasValue);
        }

        private async Task<bool> ClinicExists(Guid clinicId, CancellationToken cancellationToken)
        {
            return await _ctx.ClinicRepository.ExistsAsync(c => c.Id == clinicId, cancellationToken);
        }

        private async Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken)
        {
            return await _ctx.DoctorRepository.ExistsAsync(d => d.Id == doctorId, cancellationToken);
        }
    }
}
