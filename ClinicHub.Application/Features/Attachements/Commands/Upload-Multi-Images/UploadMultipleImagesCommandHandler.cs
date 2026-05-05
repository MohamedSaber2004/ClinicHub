using MediatR;
using ClinicHub.Application.Common.Interfaces;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Images
{
    public class UploadMultipleImagesCommandHandler : IRequestHandler<UploadMultipleImagesCommand, List<string>>
    {
        private readonly IImageValidator _imageValidator;

        public UploadMultipleImagesCommandHandler(IImageValidator imageValidator)
        {
            _imageValidator = imageValidator;
        }

        public async Task<List<string>> Handle(UploadMultipleImagesCommand request, CancellationToken cancellationToken)
        {
            var (uploaded, result) = await _imageValidator.UploadMultipleImage(request.Files, request.Place);
            
            if (uploaded)
            {
                return result.Split(',').ToList();
            }

            return new List<string>();
        }
    }
}
