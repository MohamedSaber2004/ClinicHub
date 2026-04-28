using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class UserFbToken : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = null!;
        
        public virtual ApplicationUser User { get; private set; } = null!;

        public static UserFbToken Create(Guid userId, string token) => new()
        {
            UserId = userId,
            Token = token
        };
    }
}
