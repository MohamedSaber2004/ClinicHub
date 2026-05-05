using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadImage
{
    public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, string>
    {
        private readonly IImageValidator _imageValidator;

        public UploadImageCommandHandler(IImageValidator imageValidator)
        {
            _imageValidator = imageValidator;
        }

        public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            var (uploaded, result) = await _imageValidator.UploadImage(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
