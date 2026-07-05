using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces;

namespace ClinicHub.Infrastructure.Services
{
    public class NotificationBuilderService : INotificationBuilder
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationBuilderService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<NotificationPayload> BuildAsync(NotificationType type, Guid userId, Dictionary<string, object>? parameters = null)
        {
            string title, body;
            var data = new Dictionary<string, string> { ["type"] = type.ToString() };

            var clinicName = GetParam(parameters, "clinicName");
            var senderName = GetParam(parameters, "senderName");
            var amount = GetParam(parameters, "amount");
            var date = GetParam(parameters, "date");
            var time = GetParam(parameters, "time");
            var reason = GetParam(parameters, "reason", "لا يوجد سبب");
            var message = GetParam(parameters, "message", "تحديث من النظام");
            var conversationId = GetParam(parameters, "conversationId");
            var appointmentId = GetParam(parameters, "appointmentId");

            switch (type)
            {
                case NotificationType.AppointmentReminder:
                    title = "تذكير بالموعد";
                    body = $"لديك موعد في {clinicName} الساعة {time}";
                    data["clinicName"] = clinicName;
                    data["time"] = time;
                    break;

                case NotificationType.NewMessage:
                    title = "رسالة جديدة";
                    body = $"رسالة جديدة من {senderName}";
                    data["senderName"] = senderName;
                    data["conversationId"] = conversationId;
                    break;

                case NotificationType.PaymentConfirmation:
                    title = "تم تأكيد الدفع";
                    body = $"تم تأكيد دفعتك بقيمة {amount}";
                    data["amount"] = amount;
                    data["appointmentId"] = appointmentId;
                    break;

                case NotificationType.AppointmentConfirmation:
                    title = "تم تأكيد الموعد";
                    body = $"تم تأكيد موعدك في {clinicName} بتاريخ {date}";
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    break;

                case NotificationType.AppointmentCancellation:
                    title = "تم إلغاء الموعد";
                    body = $"تم إلغاء موعدك في {clinicName}: {reason}";
                    data["clinicName"] = clinicName;
                    data["reason"] = reason;
                    break;

                case NotificationType.SystemAnnouncement:
                    title = "إشعار";
                    body = message;
                    data["message"] = message;
                    break;

                default:
                    title = "إشعار";
                    body = "لديك إشعار جديد";
                    break;
            }

            var notification = Notification.Create(userId, "", title, "", body);
            await _notificationRepository.AddAsync(notification);

            return new NotificationPayload
            {
                Title = title,
                Body = body,
                Data = data,
                Type = type
            };
        }

        private static string GetParam(Dictionary<string, object>? parameters, string key, string defaultValue = "")
        {
            if (parameters is not null && parameters.TryGetValue(key, out var value) && value is not null)
                return value.ToString() ?? defaultValue;
            return defaultValue;
        }
    }
}
