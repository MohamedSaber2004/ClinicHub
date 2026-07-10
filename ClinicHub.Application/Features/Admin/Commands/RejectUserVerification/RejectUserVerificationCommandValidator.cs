using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Commands.RejectUserVerification
{
    public class RejectUserVerificationCommandValidator : AbstractValidator<RejectUserVerificationCommand>
    {
        public RejectUserVerificationCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(localizer[LocalizationKeys.ValidationMessages.Required.Value]);

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage(localizer[LocalizationKeys.ValidationMessages.MaxLength.Value]);
        }
    }
}
