using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Common.Interfaces
{
    public class NotificationPayload
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string> Data { get; set; } = [];
        public NotificationType Type { get; set; }
    }

    public interface INotificationBuilder
    {
        Task<NotificationPayload> BuildAsync(NotificationType type, Guid userId, Dictionary<string, object>? parameters = null);
    }
}
