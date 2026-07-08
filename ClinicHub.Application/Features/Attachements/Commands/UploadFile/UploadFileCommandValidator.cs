using ClinicHub.Application.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadFile
{
    public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.File)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileEmpty.Value]))
                .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileEmpty.Value]));

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 13).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));

            RuleFor(x => x.FileType)
                .IsInEnum().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidFileType.Value]));
        }
    }
}
