using ClinicHub.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.RealTime.Commands.DisconnectUser
{
    public class DisconnectUserCommandValidator: AbstractValidator<DisconnectUserCommand>
    {
        public DisconnectUserCommandValidator(IStringLocalizer<Message> localizer)
        {
            RuleFor(x => x.ConnectionId)
                .NotEmpty()
                .WithMessage(localizer["RealTime:ConnectionIdRequired"]);
        }
    }
}
