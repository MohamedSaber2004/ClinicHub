using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class UserFbToken : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = null!;
        public DevicePlatform DevicePlatform { get; private set; }

        public virtual ApplicationUser User { get; private set; } = null!;

        public static UserFbToken Create(Guid userId, string token, DevicePlatform devicePlatform) => new()
        {
            UserId = userId,
            Token = token,
            DevicePlatform = devicePlatform
        };

        public void UpdateToken(string token)
        {
            Token = token;
        }
    }
}
