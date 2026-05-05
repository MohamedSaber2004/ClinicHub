using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.DownloadFile
{
    public class DownloadFileCommandValidator : AbstractValidator<DownloadFileCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public DownloadFileCommandValidator(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage(_localizer["Attachments:FileNotFound"])
                .NotNull().WithMessage(_localizer["Attachments:FileNotFound"]);

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);
        }
    }
}
