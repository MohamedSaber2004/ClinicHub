using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces;

namespace ClinicHub.Infrastructure.Services
{
    public class NotificationBuilderService : INotificationBuilder
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IDeepLinkService _deepLinkService;

        public NotificationBuilderService(
            INotificationRepository notificationRepository,
            IDeepLinkService deepLinkService)
        {
            _notificationRepository = notificationRepository;
            _deepLinkService = deepLinkService;
        }

        public async Task<NotificationPayload> BuildAsync(NotificationType type, Guid userId, Dictionary<string, object>? parameters = null)
        {
            string title, body;
            string? link = null;
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
            var paymentUrl = GetParam(parameters, "paymentUrl");
            var senderUserIdParam = GetParam(parameters, "SenderUserId");

            switch (type)
            {
                case NotificationType.AppointmentReminder:
                    title = "تذكير بالموعد";
                    body = $"لديك موعد في {clinicName} الساعة {time}";
                    data["clinicName"] = clinicName;
                    data["time"] = time;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.NewMessage:
                    title = "رسالة جديدة";
                    body = $"رسالة جديدة من {senderName}";
                    data["senderName"] = senderName;
                    data["conversationId"] = conversationId;
                    if (!string.IsNullOrEmpty(conversationId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.Chat, conversationId));
                    break;

                case NotificationType.PaymentConfirmation:
                    title = "تم تأكيد الدفع";
                    body = $"تم تأكيد دفعتك بقيمة {amount}";
                    data["amount"] = amount;
                    data["appointmentId"] = appointmentId;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.AppointmentConfirmation:
                    title = "تم قبول حجزك";
                    body = $"أكمل الدفع لتأكيد موعدك في {clinicName} بتاريخ {date}";
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    data["appointmentId"] = appointmentId;
                    if (!string.IsNullOrEmpty(paymentUrl))
                        data["paymentUrl"] = paymentUrl;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.AppointmentCancellation:
                    title = "تم إلغاء الموعد";
                    body = $"تم إلغاء موعدك في {clinicName}: {reason}";
                    data["clinicName"] = clinicName;
                    data["reason"] = reason;
                    link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.SystemAnnouncement:
                    title = "إشعار";
                    body = message;
                    data["message"] = message;
                    link = _deepLinkService.GenerateLink(DeepLinkRoutes.Notifications);
                    break;

                case NotificationType.CancellationWindowClosed:
                    title = "انتهت مهلة الإلغاء";
                    body = $"انتهت مهلة الإلغاء والاسترداد لموعدك في {clinicName}";
                    data["clinicName"] = clinicName;
                    data["appointmentId"] = appointmentId;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.SubscriptionExpiring:
                    title = "اشتراكك على وشك الانتهاء";
                    body = $"اشتراك عيادة {clinicName} ينتهي في {date} — جدد الآن لاستمرار الخدمات";
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    link = _deepLinkService.GenerateLink(DeepLinkRoutes.Notifications);
                    break;

                case NotificationType.AdExpiring:
                    title = "إعلانك على وشك الانتهاء";
                    body = $"إعلان عيادة {clinicName} ينتهي في {date} — جدد الآن";
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    link = _deepLinkService.GenerateLink(DeepLinkRoutes.Notifications);
                    break;

                case NotificationType.RefundProcessed:
                    title = "تم رد المبلغ";
                    body = $"تم إرجاع مبلغ {amount} الخاص بحجزك في {clinicName}";
                    data["clinicName"] = clinicName;
                    data["amount"] = amount;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.AppointmentOutsideAvailability:
                    title = "موعد خارج مواعيد الطبيب";
                    body = $"موعد {clinicName} بتاريخ {date} الساعة {time} خارج فترات توفر الطبيب — يرجى المراجعة";
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    data["time"] = time;
                    data["appointmentId"] = appointmentId;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.AppointmentOutsideWorkingHours:
                    title = "موعد خارج ساعات عمل العيادة";
                    body = $"موعد {clinicName} بتاريخ {date} الساعة {time} خارج ساعات عمل العيادة — يرجى المراجعة";
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    data["time"] = time;
                    data["appointmentId"] = appointmentId;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.NewBookingRequest:
                    title = "حجز جديد";
                    body = $"قام {GetParam(parameters, "patientName")} بحجز موعد في {clinicName} بتاريخ {date} الساعة {time}";
                    data["patientName"] = GetParam(parameters, "patientName");
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    data["time"] = time;
                    data["appointmentId"] = appointmentId;
                    data["doctorName"] = GetParam(parameters, "doctorName");
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.ClinicRegistered:
                    title = "تسجيل عيادة جديدة";
                    body = $"تسجيل عيادة جديدة بانتظار الموافقة: {clinicName}";
                    data["clinicName"] = clinicName;
                    data["clinicId"] = GetParam(parameters, "clinicId");
                    data["ownerName"] = GetParam(parameters, "ownerName");
                    if (!string.IsNullOrEmpty(GetParam(parameters, "clinicId")))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.ClinicDetails, GetParam(parameters, "clinicId")));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Clinics);
                    break;

                case NotificationType.ClinicApproved:
                    title = "تمت الموافقة على العيادة";
                    body = $"تمت الموافقة على عيادة {clinicName} — يمكنك الآن بدء العمل";
                    data["clinicName"] = clinicName;
                    data["clinicId"] = GetParam(parameters, "clinicId");
                    if (!string.IsNullOrEmpty(GetParam(parameters, "clinicId")))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.ClinicDetails, GetParam(parameters, "clinicId")));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Clinics);
                    break;

                case NotificationType.ClinicRejected:
                    title = "تم رفض العيادة";
                    body = $"عذرًا، تم رفض تسجيل عيادة {clinicName}: {reason}";
                    data["clinicName"] = clinicName;
                    data["reason"] = reason;
                    link = _deepLinkService.GenerateLink(DeepLinkRoutes.Clinics);
                    break;

                case NotificationType.SupportTicketUpdate:
                    title = "تحديث تذكرة الدعم";
                    body = $"تم تحديث حالة تذكرتك «{GetParam(parameters, "subject")}» إلى {GetParam(parameters, "status", "جديدة")}";
                    data["ticketId"] = GetParam(parameters, "ticketId");
                    data["subject"] = GetParam(parameters, "subject");
                    data["status"] = GetParam(parameters, "status");
                    if (!string.IsNullOrEmpty(GetParam(parameters, "ticketId")))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.SupportTickets, GetParam(parameters, "ticketId")));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Notifications);
                    break;

                case NotificationType.PaymentReceived:
                    title = "تم استلام الدفع";
                    body = $"تم استلام دفعة بقيمة {amount} لحجز {GetParam(parameters, "patientName")} في عيادتك {clinicName}";
                    data["amount"] = amount;
                    data["patientName"] = GetParam(parameters, "patientName");
                    data["clinicName"] = clinicName;
                    data["appointmentId"] = appointmentId;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.RevenueIncreased:
                    title = "زيادة الإيرادات";
                    body = $"تم دفع {amount} لحجز في عيادة {clinicName} — إجمالي الإيرادات الآن {GetParam(parameters, "totalRevenue")}";
                    data["amount"] = amount;
                    data["clinicName"] = clinicName;
                    data["totalRevenue"] = GetParam(parameters, "totalRevenue");
                    data["appointmentId"] = appointmentId;
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.AppointmentAccepted:
                    title = "تم تأكيد الحجز";
                    body = $"تم تأكيد حجز {GetParam(parameters, "patientName")} في {clinicName} بتاريخ {date} الساعة {time}";
                    data["patientName"] = GetParam(parameters, "patientName");
                    data["clinicName"] = clinicName;
                    data["date"] = date;
                    data["time"] = time;
                    data["appointmentId"] = appointmentId;
                    data["doctorName"] = GetParam(parameters, "doctorName");
                    if (!string.IsNullOrEmpty(appointmentId))
                        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
                    else
                        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
                    break;

                case NotificationType.SubscriptionActivated:
                    title = "تفعيل اشتراك";
                    body = $"تم تفعيل اشتراك عيادة {clinicName} في باقة {GetParam(parameters, "planName")} بقيمة {amount}";
                    data["clinicName"] = clinicName;
                    data["planName"] = GetParam(parameters, "planName");
                    data["amount"] = amount;
                    link = _deepLinkService.GenerateLink(DeepLinkRoutes.Notifications);
                    break;

                default:
                    title = "إشعار";
                    body = "لديك إشعار جديد";
                    break;
            }

            data["link"] = link ?? string.Empty;

            Guid? senderUserId = Guid.TryParse(senderUserIdParam, out var parsedSender) ? parsedSender : null;
            var notification = Notification.Create(userId, senderUserId, "", title, "", body, type);
            await _notificationRepository.AddAsync(notification);

            return new NotificationPayload
            {
                Title = title,
                Body = body,
                Data = data,
                Type = type,
                Link = link
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
