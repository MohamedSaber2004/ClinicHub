using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateImage
{
    public class UpdateImageCommandHandler : IRequestHandler<UpdateImageCommand, string>
    {
        private readonly IImageValidator _imageValidator;

        public UpdateImageCommandHandler(IImageValidator imageValidator)
        {
            _imageValidator = imageValidator;
        }

        public async Task<string> Handle(UpdateImageCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.OldFileName))
            {
                await _imageValidator.DeleteImage(request.OldFileName, request.Place);
            }

            var (uploaded, result) = await _imageValidator.UploadImage(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
