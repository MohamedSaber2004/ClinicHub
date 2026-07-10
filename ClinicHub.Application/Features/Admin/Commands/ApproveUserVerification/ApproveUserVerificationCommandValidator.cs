using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Commands.ApproveUserVerification
{
    public class ApproveUserVerificationCommandValidator : AbstractValidator<ApproveUserVerificationCommand>
    {
        public ApproveUserVerificationCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);
        }
    }
}
