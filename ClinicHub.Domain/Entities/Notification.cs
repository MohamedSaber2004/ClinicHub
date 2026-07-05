using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Notification : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid UserId { get; private set; }
        public string TitleEn { get; private set; } = null!;
        public string TitleAr { get; private set; } = null!;
        public string BodyEn { get; private set; } = null!;
        public string BodyAr { get; private set; } = null!;
        public NotificationType Type { get; private set; }
        public bool IsRead { get; private set; } = false;

        public Guid? ClinicId { get; private set; }
        public Clinic? Clinic { get; private set; }

        public virtual ApplicationUser User { get; private set; } = null!;

        public static Notification Create(Guid userId, Guid? senderUserId, string titleEn, string titleAr, string bodyEn, string bodyAr, NotificationType type, Guid? clinicId = null) => new()
        {
            UserId = userId,
            TitleEn = titleEn,
            TitleAr = titleAr,
            BodyEn = bodyEn,
            BodyAr = bodyAr,
            Type = type,
            ClinicId = clinicId
        };

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
