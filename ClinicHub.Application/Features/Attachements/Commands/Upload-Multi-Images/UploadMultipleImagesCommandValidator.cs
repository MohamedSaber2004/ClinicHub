using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Images
{
    public class UploadMultipleImagesCommandValidator : AbstractValidator<UploadMultipleImagesCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public UploadMultipleImagesCommandValidator(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;

            RuleFor(x => x.Files)
                .NotEmpty().WithMessage(_localizer["Attachments:FileEmpty"])
                .NotNull().WithMessage(_localizer["Attachments:FileEmpty"]);

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);
        }
    }
}
