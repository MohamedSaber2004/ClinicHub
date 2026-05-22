using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Queries.GetUserProfile
{
    public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserProfileQueryValidator(
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Messages> localizer)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;

            RuleFor(x => x)
                .Must(x => _currentUserService.IsAuthenticated)
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ExceptionMessages.Unauthorized.Value]))
                .MustAsync(async (x, cancellationToken) =>
                {
                    var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
                    return user != null;
                })
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]));
        }
    }
}
