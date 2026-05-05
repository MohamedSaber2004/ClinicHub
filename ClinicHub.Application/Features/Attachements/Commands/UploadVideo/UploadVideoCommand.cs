using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadVideo
{
    public class UploadVideoCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = null!;
        public int Place { get; set; }
    }
}
