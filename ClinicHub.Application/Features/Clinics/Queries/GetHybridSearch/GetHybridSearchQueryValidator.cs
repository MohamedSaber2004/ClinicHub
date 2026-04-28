using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch
{
    public class GetHybridSearchQueryValidator : AbstractValidator<GetHybridSearchQuery>
    {
        public GetHybridSearchQueryValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.UserLat)
                .InclusiveBetween(-90, 90)
                .When(x => x.IsNearest)
                .WithMessage(localizer["Clinics.InvalidLatitude"]);

            RuleFor(x => x.UserLng)
                .InclusiveBetween(-180, 180)
                .When(x => x.IsNearest)
                .WithMessage(localizer["Clinics.InvalidLongitude"]);

            //RuleFor(x => x.RadiusInKm)
            //    .InclusiveBetween(0.1, 5)
            //    .When(x => x.IsNearest)
            //    .WithMessage(localizer["Clinics.InvalidRadius"]);
        }
    }
}
