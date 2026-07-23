using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.DoctorDashboard.Queries.GetPatientHistory
{
    public class GetPatientHistoryQueryValidator : AbstractValidator<GetPatientHistoryQuery>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetPatientHistoryQueryValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(v => v.PatientUserId)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, ct) =>
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    return user != null && !user.IsDeleted;
                }).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.UserNotFound.Value]));

            RuleFor(v => v.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageNumberMustBeGreaterThanOrEqualToOne.Value]));

            RuleFor(v => v.PageSize)
                .InclusiveBetween(1, 100).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.PageSizeMustBeLessThanOrEqualToHundred.Value]));
        }
    }
}
