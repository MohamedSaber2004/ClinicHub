using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadFile
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
    {
        private readonly IFileValidator _fileValidator;

        public UploadFileCommandHandler(IFileValidator fileValidator)
        {
            _fileValidator = fileValidator;
        }

        public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var (uploaded, result) = await _fileValidator.UploadFile(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
