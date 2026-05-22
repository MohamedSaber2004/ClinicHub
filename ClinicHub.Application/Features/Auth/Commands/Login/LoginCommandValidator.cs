using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginCommandValidator(UserManager<ApplicationUser> userManager, IStringLocalizer<Messages> localizer)
        {
            _userManager = userManager;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .EmailAddress().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Value]));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]));
            RuleFor(x => x)
                .CustomAsync(async (request, context, cancellationToken) =>
                {
                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                    {
                        context.AddFailure(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.InvalidCredentials.Value]));
                    }
                    else if (!user.EmailConfirmed)
                    {
                        context.AddFailure(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AuthMessages.AccountNotVerified.Value]));
                    }
                });
        }
    }
}
