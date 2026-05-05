using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadAudio
{
    public class UploadAudioCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = null!;
        public int Place { get; set; }
    }
}
