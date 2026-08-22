using ClinicHub.Domain.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ClinicHub.Domain.Common
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; internal set; }
        public DateTime? UpdatedAt { get; internal set; }
        public DateTime? DeletedAt { get; internal set; }
        public string CreatedBy { get; internal set; } = string.Empty;
        public string? UpdatedBy { get; internal set; }
        public string? DeletedBy { get; internal set; }
        public bool IsDeleted { get; private set; }
        public bool IsActive { get; private set; } = true;
        [Timestamp]
        public byte[]? Version { get; internal set; }

        private static readonly TimeZoneInfo AppTimeZone = ResolveAppTimeZone();

        /// <summary>
        /// Wall-clock time in Africa/Cairo regardless of the host server's timezone.
        /// The production host runs UTC+2 while Egypt observes DST (UTC+3), which made
        /// DateTime.Now stamps lag an hour behind users' clocks during DST months.
        /// </summary>
        public static DateTime CairoNow => TimeZoneInfo.ConvertTime(DateTime.UtcNow, AppTimeZone);

        private static TimeZoneInfo ResolveAppTimeZone()
        {
            foreach (var id in new[] { "Egypt Standard Time", "Africa/Cairo" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Local;
        }

        public void Deactive()
        {
            IsActive = false;
            IsDeleted = true;
        }

        public void Active()
        {
            IsActive = true;
            IsDeleted = false;
        }

        public void SetActiveState(bool isActive, string updatedBy)
        {
            IsActive = isActive;
            MarkAsUpdated(updatedBy);
        }

        public virtual void MarkAsDeleted(string deletedBy)
        {
            IsDeleted = true;
            IsActive = false;
            DeletedAt = CairoNow;
            DeletedBy = deletedBy;
        }

        public virtual void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = CairoNow;
            UpdatedBy = updatedBy;
        }

        public virtual void MarkAsCreated(string createdBy)
        {
            CreatedAt = CairoNow;
            CreatedBy = createdBy;
            IsActive = true;
            IsDeleted = false;
        }
    }

    public class BaseEntity<TKey> : BaseEntity, IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        [Key]
        public TKey Id { get; protected set; } = default!;

        public BaseEntity()
        {
            if (typeof(TKey) == typeof(Guid))
            {
                Id = (TKey)(object)Guid.NewGuid();
            }
            Active();
        }
    }
}
