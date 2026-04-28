using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class Notification : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public string TitleEn { get; private set; } = null!;
        public string TitleAr { get; private set; } = null!;
        public string BodyEn { get; private set; } = null!;
        public string BodyAr { get; private set; } = null!;
        public bool IsRead { get; private set; } = false;
        
        public virtual ApplicationUser User { get; private set; } = null!;

        public static Notification Create(Guid userId, string titleEn, string titleAr, string bodyEn, string bodyAr) => new()
        {
            UserId = userId,
            TitleEn = titleEn,
            TitleAr = titleAr,
            BodyEn = bodyEn,
            BodyAr = bodyAr
        };

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
