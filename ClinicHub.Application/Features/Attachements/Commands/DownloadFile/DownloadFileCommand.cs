using MediatR;
using ClinicHub.Application.Common.Models;

namespace ClinicHub.Application.Features.Attachements.Commands.DownloadFile
{
    public class DownloadFileCommand: IRequest<FileResponseDto>
    {
        public int Place { get; set; }
        public string FileName { get; set; } = null!;
    }
}
