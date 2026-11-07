using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Commands.SendTestPushNotification
{
    public class SendTestPushCommandValidator : AbstractValidator<SendTestPushCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SendTestPushCommandValidator(IStringLocalizer<Messages> localizer, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, cancellationToken) =>
                    await _userManager.Users.AnyAsync(u => u.Id == id && !u.IsDeleted, cancellationToken))
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.UserNotFound.Value]));

            RuleFor(x => x.Title!)
                .MaximumLength(120)
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));

            RuleFor(x => x.Body!)
                .MaximumLength(500)
                    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]));
        }
    }
}
