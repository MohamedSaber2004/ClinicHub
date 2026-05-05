using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateFile
{
    public class UpdateFileCommandHandler : IRequestHandler<UpdateFileCommand, string>
    {
        private readonly IFileValidator _fileValidator;

        public UpdateFileCommandHandler(IFileValidator fileValidator)
        {
            _fileValidator = fileValidator;
        }

        public async Task<string> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.OldFileName))
            {
                await _fileValidator.DeleteFile(request.OldFileName, request.Place);
            }

            var (uploaded, result) = await _fileValidator.UploadFile(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
