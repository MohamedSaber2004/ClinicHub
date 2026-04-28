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
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key])
                .EmailAddress().WithMessage(localizer[LocalizationKeys.ValidationMessages.InvalidEmail.Key]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Key]);

            RuleFor(x => x)
                .CustomAsync(async (request, context, cancellationToken) =>
                {
                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                    {
                        context.AddFailure(localizer[LocalizationKeys.AuthMessages.InvalidCredentials.Key]);
                    }
                    else if (!user.EmailConfirmed)
                    {
                        context.AddFailure(localizer[LocalizationKeys.AuthMessages.AccountNotVerified.Key]);
                    }
                });
        }
    }
}
