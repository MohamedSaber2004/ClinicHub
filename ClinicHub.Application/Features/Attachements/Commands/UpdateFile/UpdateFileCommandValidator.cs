using FluentValidation;
using Microsoft.Extensions.Localization;
using ClinicHub.Application.Localization;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateFile
{
    public class UpdateFileCommandValidator : AbstractValidator<UpdateFileCommand>
    {
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly IFileValidator _fileValidator;

        public UpdateFileCommandValidator(IStringLocalizer<Messages> localizer, IFileValidator fileValidator)
        {
            _localizer = localizer;
            _fileValidator = fileValidator;

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
                    return _fileValidator.FileIsExisted(Path.Combine(path!, oldFileName));
                })
                .WithMessage(_localizer["Attachments:FileNotFound"]);
        }
    }
}
