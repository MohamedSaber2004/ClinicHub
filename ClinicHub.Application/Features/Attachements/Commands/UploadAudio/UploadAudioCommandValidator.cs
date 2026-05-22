using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadAudio
{
    public class UploadAudioCommandValidator : AbstractValidator<UploadAudioCommand>
    {
        public UploadAudioCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.File)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileEmpty.Value]))
                .NotNull().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileEmpty.Value]));

            RuleFor(x => x.Place)
                .InclusiveBetween(0, 12).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));
        }
    }
}
