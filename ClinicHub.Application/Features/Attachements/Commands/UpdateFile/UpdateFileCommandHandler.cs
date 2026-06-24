using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateFile
{
    public class UpdateFileCommandHandler : IRequestHandler<UpdateFileCommand, string>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;

        public UpdateFileCommandHandler(
            IImageValidator imageValidator,
            IAudioValidator audioValidator,
            IVideoValidator videoValidator,
            IFileValidator fileValidator)
        {
            _imageValidator = imageValidator;
            _audioValidator = audioValidator;
            _videoValidator = videoValidator;
            _fileValidator = fileValidator;
        }

        public async Task<string> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.OldFileName))
            {
                await (request.FileType switch
                {
                    MediaType.Image => _imageValidator.DeleteImage(request.OldFileName, request.Place),
                    MediaType.Audio => _audioValidator.DeleteAudio(request.OldFileName, request.Place),
                    MediaType.Video => _videoValidator.DeleteVideo(request.OldFileName, request.Place),
                    MediaType.File => _fileValidator.DeleteFile(request.OldFileName, request.Place),
                    _ => Task.CompletedTask
                });
            }

            (bool Uploaded, string Result) result = request.FileType switch
            {
                MediaType.Image => await _imageValidator.UploadImage(request.File, request.Place),
                MediaType.Audio => await _audioValidator.UploadAudio(request.File, request.Place),
                MediaType.Video => await _videoValidator.UploadVideo(request.File, request.Place),
                MediaType.File => await _fileValidator.UploadFile(request.File, request.Place),
                _ => throw new BadRequestException(LocalizationKeys.AttachmentMessages.InvalidFileType.Value)
            };

            if (!result.Uploaded)
                throw new BadRequestException(result.Result);

            return result.Result;
        }
    }
}
