using FluentValidation;

namespace ClinicHub.Application.Features.Auth.Commands.UnregisterDeviceToken
{
    public class UnregisterDeviceTokenCommandValidator : AbstractValidator<UnregisterDeviceTokenCommand>
    {
        public UnregisterDeviceTokenCommandValidator()
        {
            RuleFor(x => x.FcmToken).NotEmpty();
        }
    }
}

