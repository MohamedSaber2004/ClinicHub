using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Attachments
{
    public class UploadMultipleAttachmentsCommand : IRequest<List<string>>
    {
        public List<IFormFile>? Images { get; set; }
        public int ImagesPlace { get; set; }

        public List<IFormFile>? Videos { get; set; }
        public int VideosPlace { get; set; }

        public List<IFormFile>? Audios { get; set; }
        public int AudiosPlace { get; set; }

        public List<IFormFile>? Documents { get; set; }
        public int DocumentsPlace { get; set; }
    }
}
