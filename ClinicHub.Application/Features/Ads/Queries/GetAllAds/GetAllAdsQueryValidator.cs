using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Ads.Queries.GetAllAds;

public class GetAllAdsQueryValidator : AbstractValidator<GetAllAdsQuery>
{
    public GetAllAdsQueryValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(v => v.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]);

        RuleFor(v => v.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeGreaterThanOrEqualToOne.Value])
            .LessThanOrEqualTo(100).WithMessage(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]);
    }
}
