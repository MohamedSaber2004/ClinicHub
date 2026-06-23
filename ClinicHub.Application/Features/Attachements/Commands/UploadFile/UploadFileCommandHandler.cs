using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadFile
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;

        public UploadFileCommandHandler(
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

        public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            (bool Uploaded, string Result) result = request.FileType switch
            {
                MediaType.Image => await _imageValidator.UploadImage(request.File, request.Place),
                MediaType.Audio => await _audioValidator.UploadAudio(request.File, request.Place),
                MediaType.Video => await _videoValidator.UploadVideo(request.File, request.Place),
                MediaType.File => await _fileValidator.UploadFile(request.File, request.Place),
                _ => throw new BadRequestException("Invalid file type")
            };

            if (!result.Uploaded)
                throw new BadRequestException(result.Result);

            return result.Result;
        }
    }
}
