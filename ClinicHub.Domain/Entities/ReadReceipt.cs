using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class ReadReceipt : BaseEntity<Guid>
    {
        public Guid MessageId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime ReadAt { get; private set; }

        public Message Message { get; private set; } = null!;

        private ReadReceipt()
        {
        }

        public ReadReceipt(Guid messageId, Guid userId, DateTime readAt)
        {
            MessageId = messageId;
            UserId = userId;
            ReadAt = readAt;
        }
    }
}
