using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateVideo
{
    public class UpdateVideoCommandValidator : AbstractValidator<UpdateVideoCommand>
    {
        private readonly IVideoValidator _videoValidator;

        public UpdateVideoCommandValidator(IStringLocalizer<Messages> localizer, IVideoValidator videoValidator)
        {
            _videoValidator = videoValidator;

            RuleFor(x => x.File)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileEmpty.Value]))
                .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileEmpty.Value]));

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));

            RuleFor(x => x.OldFileName)
                .Must((command, oldFileName) =>
                {
                    if (string.IsNullOrEmpty(oldFileName)) return true;
                    var path = UploadPaths.GetPath(command.Place);
                    return _videoValidator.VideoIsExisted(Path.Combine(path!, oldFileName));
                })
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileNotFound.Value]));
        }
    }
}
