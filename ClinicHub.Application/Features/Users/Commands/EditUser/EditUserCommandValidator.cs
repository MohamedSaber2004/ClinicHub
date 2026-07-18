using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Users.Commands.EditUser
{
    public class EditUserCommandValidator : AbstractValidator<EditUserCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EditUserCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.Id).NotEmpty()
                .MustAsync((userId, cancellationToken) => UserExists(userId, cancellationToken))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]));
        }

        private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
        {
            return await _userManager.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == userId, cancellationToken);
        }
    }
}
