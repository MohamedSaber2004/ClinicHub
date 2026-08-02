using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPaymentStats;

public class GetAdminPaymentStatsQueryValidator : AbstractValidator<GetAdminPaymentStatsQuery>
{
    public GetAdminPaymentStatsQueryValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEnumValue.Value]))
            .When(x => x.Type.HasValue);

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate.Value <= x.ToDate.Value)
            .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidDateRange.Value]))
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}
