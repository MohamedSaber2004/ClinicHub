using System.ComponentModel.DataAnnotations;

namespace ClinicHub.Domain.Common.Interfaces
{
    public interface IBaseEntity<TKey>
    {
        [Key]
        public TKey Id { get; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; }
        public DateTime? DeletedAt { get; }
        public string CreatedBy { get; }
        public string? UpdatedBy { get; }
        public string? DeletedBy { get; }
        public bool IsDeleted { get; }
        public bool IsActive { get; }
    }
}
