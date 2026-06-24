using FluentValidation;

namespace ClinicHub.Application.Features.RealTime.Commands.DisconnectUser
{
    public class DisconnectUserCommandValidator: AbstractValidator<DisconnectUserCommand>
    {
        public DisconnectUserCommandValidator()
        {
            RuleFor(x => x.ConnectionId)
                .NotEmpty();
        }
    }
}
