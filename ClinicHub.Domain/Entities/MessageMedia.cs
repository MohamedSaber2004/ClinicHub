using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class MessageMedia : BaseEntity<Guid>
    {
        public Guid MessageId { get; private set; }
        public MediaType MediaType { get; private set; }
        public string? FileName { get; private set; }

        public Message Message { get; private set; } = null!;

        private MessageMedia()
        {
        }

        public MessageMedia(Guid messageId, MediaType mediaType, 
            string? fileName = null, string? mimeType = null)
        {
            MessageId = messageId;
            MediaType = mediaType;
            FileName = fileName;
        }
    }
}
