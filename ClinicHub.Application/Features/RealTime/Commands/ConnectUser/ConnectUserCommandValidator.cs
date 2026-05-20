using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.ConnectUser
{
    public class ConnectUserCommandValidator: AbstractValidator<ConnectUserCommand>
    {
        public ConnectUserCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.ConnectionId)
                .NotEmpty()
                .WithMessage(localizer["RealTime:ConnectionIdRequired"]);
        }
    }
}
