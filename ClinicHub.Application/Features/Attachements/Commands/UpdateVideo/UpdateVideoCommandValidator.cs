using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateVideo
{
    public class UpdateVideoCommandValidator : AbstractValidator<UpdateVideoCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IVideoValidator _videoValidator;

        public UpdateVideoCommandValidator(IStringLocalizer<Messages> localizer, IVideoValidator videoValidator)
        {
            _localizer = localizer;
            _videoValidator = videoValidator;

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
                    return _videoValidator.VideoIsExisted(Path.Combine(path!, oldFileName));
                })
                .WithMessage(_localizer["Attachments:FileNotFound"]);
        }
    }
}
