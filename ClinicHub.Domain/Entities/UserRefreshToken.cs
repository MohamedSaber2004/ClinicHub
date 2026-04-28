using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class UserRefreshToken : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = null!;
        public DateTime ExpiryDate { get; private set; }
        public bool IsRevoked { get; private set; } = false;

        public virtual ApplicationUser User { get; private set; } = null!;

        public static UserRefreshToken Create(Guid userId, string token, DateTime expiryDate) => new()
        {
            UserId = userId,
            Token = token,
            ExpiryDate = expiryDate
        };

        public void Revoke() => IsRevoked = true;
        
        public bool IsValid => !IsRevoked && ExpiryDate > DateTime.UtcNow;
    }
}
