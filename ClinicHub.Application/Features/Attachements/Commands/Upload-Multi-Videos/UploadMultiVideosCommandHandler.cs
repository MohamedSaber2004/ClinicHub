using MediatR;
using ClinicHub.Application.Common.Interfaces;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Videos
{
    public class UploadMultiVideosCommandHandler : IRequestHandler<UploadMultiVideosCommand, List<string>>
    {
        private readonly IVideoValidator _videoValidator;

        public UploadMultiVideosCommandHandler(IVideoValidator videoValidator)
        {
            _videoValidator = videoValidator;
        }

        public async Task<List<string>> Handle(UploadMultiVideosCommand request, CancellationToken cancellationToken)
        {
            var (uploaded, result) = await _videoValidator.UploadMultipleVideo(request.Files, request.Place);
            
            if (uploaded)
            {
                return result.Split(',').ToList();
            }

            return new List<string>();
        }
    }
}
