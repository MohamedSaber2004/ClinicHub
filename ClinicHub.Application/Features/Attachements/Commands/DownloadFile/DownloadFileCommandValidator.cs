using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.DownloadFile
{
    public class DownloadFileCommandValidator : AbstractValidator<DownloadFileCommand>
    {
        public DownloadFileCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileNotFound.Value]))
                .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileNotFound.Value]));

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));
        }
    }
}
