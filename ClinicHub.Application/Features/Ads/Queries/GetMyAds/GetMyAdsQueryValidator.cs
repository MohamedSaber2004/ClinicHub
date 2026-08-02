using FluentValidation;

namespace ClinicHub.Application.Features.Ads.Queries.GetMyAds;

public class GetMyAdsQueryValidator : AbstractValidator<GetMyAdsQuery>
{
    public GetMyAdsQueryValidator()
    {
        RuleFor(v => v.ClinicId).NotEmpty();
    }
}
