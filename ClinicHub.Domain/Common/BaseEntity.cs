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

        public void Deactive() => IsActive = false;
        public void Active() => IsActive = true;

        public virtual void MarkAsDeleted(string deletedBy)
        {
            IsDeleted = true;
            IsActive = false;
            DeletedAt = DateTime.Now;
            DeletedBy = deletedBy;
        }

        public virtual void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = DateTime.Now;
            UpdatedBy = updatedBy;
        }

        public virtual void MarkAsCreated(string createdBy)
        {
            CreatedAt = DateTime.Now;
            CreatedBy = createdBy;
            IsActive = true;
            IsDeleted = false;
        }
    }

    public class BaseEntity<TKey> : BaseEntity, IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        [Key]
        public TKey Id { get; private set; } = default!;

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
