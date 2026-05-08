using MediatR;
using ClinicHub.Application.Common.Interfaces;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Files
{
    public class UploadMultipleFilesCommandHandler : IRequestHandler<UploadMultipleFilesCommand, List<string>>
    {
        private readonly IFileValidator _fileValidator;

        public UploadMultipleFilesCommandHandler(IFileValidator fileValidator)
        {
            _fileValidator = fileValidator;
        }

        public async Task<List<string>> Handle(UploadMultipleFilesCommand request, CancellationToken cancellationToken)
        {
            var (uploaded, result) = await _fileValidator.UploadMultipleFile(request.Files, request.Place);

            if (uploaded)
            {
                return result.Split(',').ToList();
            }

            return new List<string>();
        }
    }
}
