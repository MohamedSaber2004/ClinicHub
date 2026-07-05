using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IFcmService
    {
        Task SendToUserAsync(Guid userId, NotificationType type, Dictionary<string, object>? parameters = null);
        Task SendToDeviceAsync(string deviceToken, NotificationPayload payload, DevicePlatform platform);
        Task RegisterTokenAsync(Guid userId, string token, DevicePlatform platform);
        Task UnregisterTokenAsync(string token);
    }
}
