using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadVideo
{
    public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, string>
    {
        private readonly IVideoValidator _videoValidator;

        public UploadVideoCommandHandler(IVideoValidator videoValidator)
        {
            _videoValidator = videoValidator;
        }

        public async Task<string> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
        {
            var (uploaded, result) = await _videoValidator.UploadVideo(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
