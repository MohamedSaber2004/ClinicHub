using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWithFacebook
{
    public sealed class LoginWithFacebookCommandValidator : AbstractValidator<LoginWithFacebookCommand>
    {
        public LoginWithFacebookCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.AuthMessages.FacebookTokenRequired.Value]);
        }
    }
}
