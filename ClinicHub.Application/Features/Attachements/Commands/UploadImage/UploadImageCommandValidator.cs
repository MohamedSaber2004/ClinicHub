using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadImage
{
    public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public UploadImageCommandValidator(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;

            RuleFor(x => x.File)
                .NotEmpty().WithMessage(_localizer["Attachments:FileEmpty"])
                .NotNull().WithMessage(_localizer["Attachments:FileEmpty"]);

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);
        }
    }
}
