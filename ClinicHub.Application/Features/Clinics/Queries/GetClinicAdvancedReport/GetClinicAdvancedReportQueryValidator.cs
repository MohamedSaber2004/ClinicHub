using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicAdvancedReport
{
    public class GetClinicAdvancedReportQueryValidator : AbstractValidator<GetClinicAdvancedReportQuery>
    {
        public GetClinicAdvancedReportQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(q => q.To)
                .GreaterThanOrEqualTo(q => q.From)
                .When(q => q.From.HasValue && q.To.HasValue)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidDateRange.Value]));
        }
    }
}