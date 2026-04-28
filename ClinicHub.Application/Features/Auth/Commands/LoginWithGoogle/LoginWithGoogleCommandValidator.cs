using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.LoginWithGoogle
{
    public sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
    {
        public LoginWithGoogleCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.AuthMessages.GoogleTokenRequired.Value]);
        }
    }
}
