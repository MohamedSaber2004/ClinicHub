using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Attachments
{
    public class UploadMultipleAttachmentsCommandValidator : AbstractValidator<UploadMultipleAttachmentsCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public UploadMultipleAttachmentsCommandValidator(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;

            // Check that at least one category has files
            RuleFor(x => x)
                .Must(x => (x.Images != null && x.Images.Any()) || 
                           (x.Videos != null && x.Videos.Any()) || 
                           (x.Audios != null && x.Audios.Any()) || 
                           (x.Documents != null && x.Documents.Any()))
                .WithMessage(_localizer["Attachments:FileEmpty"]);

            RuleFor(x => x.ImagesPlace)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);

            RuleFor(x => x.VideosPlace)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);

            RuleFor(x => x.AudiosPlace)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);

            RuleFor(x => x.DocumentsPlace)
                .InclusiveBetween(0, 12).WithMessage(_localizer["Attachments:InvalidPlace"]);
        }
    }
}
