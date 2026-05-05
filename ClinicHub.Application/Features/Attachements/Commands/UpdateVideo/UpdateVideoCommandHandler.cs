using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateVideo
{
    public class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand, string>
    {
        private readonly IVideoValidator _videoValidator;

        public UpdateVideoCommandHandler(IVideoValidator videoValidator)
        {
            _videoValidator = videoValidator;
        }

        public async Task<string> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.OldFileName))
            {
                await _videoValidator.DeleteVideo(request.OldFileName, request.Place);
            }

            var (uploaded, result) = await _videoValidator.UploadVideo(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
