using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Queries.GetUrgentSupportTickets
{
    public class GetUrgentSupportTicketsQueryValidator : AbstractValidator<GetUrgentSupportTicketsQuery>
    {
        public GetUrgentSupportTicketsQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]));

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeGreaterThanOrEqualToOne.Value]))
                .LessThanOrEqualTo(100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]));
        }
    }
}
