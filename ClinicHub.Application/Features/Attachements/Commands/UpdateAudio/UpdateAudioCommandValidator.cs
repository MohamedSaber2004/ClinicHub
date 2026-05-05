using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateAudio
{
    public class UpdateAudioCommandValidator : AbstractValidator<UpdateAudioCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IAudioValidator _audioValidator;

        public UpdateAudioCommandValidator(IStringLocalizer<Messages> localizer, IAudioValidator audioValidator)
        {
            _localizer = localizer;
            _audioValidator = audioValidator;

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
                    return _audioValidator.AudioIsExisted(Path.Combine(path!, oldFileName));
                })
                .WithMessage(_localizer["Attachments:FileNotFound"]);
        }
    }
}
