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

        /// <summary>
        /// Platform is timezone-free: all timestamps are plain server wall-clock
        /// (<see cref="DateTime.Now"/>) with no conversions. The host clock is the
        /// single source of truth — no per-clinic or Cairo/UTC translation.
        /// </summary>
        public static DateTime AppNow => DateTime.Now;

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
            DeletedAt = AppNow;
            DeletedBy = deletedBy;
        }

        public virtual void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = AppNow;
            UpdatedBy = updatedBy;
        }

        public virtual void MarkAsCreated(string createdBy)
        {
            CreatedAt = AppNow;
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
