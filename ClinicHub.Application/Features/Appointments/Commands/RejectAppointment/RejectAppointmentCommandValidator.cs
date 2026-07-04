using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Appointments.Commands.RejectAppointment
{
    public class RejectAppointmentCommandValidator : AbstractValidator<RejectAppointmentCommand>
    {
        private readonly IUnitOfWork _ctx;

        public RejectAppointmentCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.AppointmentId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(AppointmentExists).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AppointmentMessages.AppointmentNotFound.Value]));

            RuleFor(v => v.RejectionReason)
                .MaximumLength(500).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }

        private async Task<bool> AppointmentExists(Guid id, CancellationToken cancellationToken)
        {
            return await _ctx.AppointmentRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        }
    }
}
