using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorRejectAppointment
{
    public class DoctorRejectAppointmentCommandValidator : AbstractValidator<DoctorRejectAppointmentCommand>
    {
        private readonly IUnitOfWork _ctx;

        public DoctorRejectAppointmentCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.AppointmentId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(AppointmentExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.AppointmentNotFound.Value]));

            RuleFor(v => v.Reason)
                .MaximumLength(500).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }

        private async Task<bool> AppointmentExists(Guid id, CancellationToken cancellationToken)
        {
            return await _ctx.AppointmentRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        }
    }
}
