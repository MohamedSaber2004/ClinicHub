using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetDoctorAppointments
{
    public class GetDoctorAppointmentsQueryValidator : AbstractValidator<GetDoctorAppointmentsQuery>
    {
        public GetDoctorAppointmentsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(v => v.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]));

            RuleFor(v => v.PageSize)
                .InclusiveBetween(1, 100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]));

            RuleFor(v => v.Status)
                .IsInEnum().When(v => v.Status.HasValue).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]));

            When(v => v.StartDate.HasValue && v.EndDate.HasValue, () =>
            {
                RuleFor(v => v.EndDate)
                    .GreaterThanOrEqualTo(v => v.StartDate).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidDateRange.Value]));
            });
        }
    }
}
