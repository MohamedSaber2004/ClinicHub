using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadFile
{
    public class UploadFileCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = null!;
        public int Place { get; set; }
    }
}
