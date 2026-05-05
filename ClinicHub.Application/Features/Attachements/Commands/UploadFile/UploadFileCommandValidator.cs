using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadFile
{
    public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public UploadFileCommandValidator(IStringLocalizer<Messages> localizer)
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
