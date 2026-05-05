using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Videos
{
    public class UploadMultiVideosCommand: IRequest<List<string>>
    {
        public List<IFormFile> Files { get; set; } = null!;
        public int Place { get; set; }
    }
}
