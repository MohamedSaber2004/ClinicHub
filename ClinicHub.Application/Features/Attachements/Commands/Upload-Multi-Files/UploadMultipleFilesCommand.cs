using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Files
{
    public class UploadMultipleFilesCommand : IRequest<List<string>>
    {
        public List<IFormFile> Files { get; set; } = null!;
        public int Place { get; set; }
    }
}
