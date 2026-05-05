using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateFile
{
    public class UpdateFileCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = null!;
        public string? OldFileName { get; set; }
        public int Place { get; set; }
    }
}
