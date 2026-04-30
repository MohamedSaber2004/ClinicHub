using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandValidator: AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.AuthMessages.RefreshTokenRequired.Value]);
        }
    }
}
