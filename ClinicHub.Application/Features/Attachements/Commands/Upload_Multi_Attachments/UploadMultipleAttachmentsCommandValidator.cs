using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Attachments
{
    public class UploadMultipleAttachmentsCommandValidator : AbstractValidator<UploadMultipleAttachmentsCommand>
    {
        public UploadMultipleAttachmentsCommandValidator(IStringLocalizer<Messages> localizer)
        {
            // Check that at least one category has files
            RuleFor(x => x)
                .Must(x => (x.Images != null && x.Images.Any()) || 
                           (x.Videos != null && x.Videos.Any()) || 
                           (x.Audios != null && x.Audios.Any()) || 
                           (x.Documents != null && x.Documents.Any()))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.FileEmpty.Value]));

            RuleFor(x => x.ImagesPlace)
                .InclusiveBetween(0, 12).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));

            RuleFor(x => x.VideosPlace)
                .InclusiveBetween(0, 12).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));

            RuleFor(x => x.AudiosPlace)
                .InclusiveBetween(0, 12).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));

            RuleFor(x => x.DocumentsPlace)
                .InclusiveBetween(0, 12).WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AttachmentMessages.InvalidPlace.Value]));
        }
    }
}
