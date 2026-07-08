using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserByIdQueryValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (id, cancellationToken) =>
                    await _userManager.Users.AnyAsync(u => u.Id == id && !u.IsDeleted, cancellationToken))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]));
        }
    }
}
