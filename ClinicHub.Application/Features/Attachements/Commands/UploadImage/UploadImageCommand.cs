using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadImage
{
    public class UploadImageCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = null!;
        public int Place { get; set; }
    }
}
