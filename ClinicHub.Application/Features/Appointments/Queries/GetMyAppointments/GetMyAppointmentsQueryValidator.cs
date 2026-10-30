using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Appointments.Queries.GetMyAppointments
{
    public class GetMyAppointmentsQueryValidator : AbstractValidator<GetMyAppointmentsQuery>
    {
        public GetMyAppointmentsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(v => v.Status)
                .Must(status => AppointmentStatusFilter.TryResolve(status, out _))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.BookingMessages.InvalidStatus.Value]));
        }
    }
}
