using ClinicHub.Domain.Enums;
using System;

namespace ClinicHub.Application.Features.Notifications.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? SenderUserId { get; set; }
        public string TitleEn { get; set; } = null!;
        public string TitleAr { get; set; } = null!;
        public string BodyEn { get; set; } = null!;
        public string BodyAr { get; set; } = null!;
        public bool IsRead { get; set; }
        public Guid? ClinicId { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationType Type { get; set; }
    }
}
