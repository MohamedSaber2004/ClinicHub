using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateImage
{
    public class UpdateImageCommandValidator : AbstractValidator<UpdateImageCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IImageValidator _imageValidator;

        public UpdateImageCommandValidator(IStringLocalizer<Messages> localizer, IImageValidator imageValidator)
        {
            _localizer = localizer;
            _imageValidator = imageValidator;

            RuleFor(x => x.File)
                .NotEmpty().WithMessage(_localizer["Attachments:FileEmpty"])
                .NotNull().WithMessage(_localizer["Attachments:FileEmpty"]);

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);

            RuleFor(x => x.OldFileName)
                .Must((command, oldFileName) =>
                {
                    if (string.IsNullOrEmpty(oldFileName)) return true;
                    var path = UploadPaths.GetPath(command.Place);
                    return _imageValidator.ImageIsExisted(Path.Combine(path!, oldFileName));
                })
                .WithMessage(_localizer["Attachments:FileNotFound"]);
        }
    }
}
