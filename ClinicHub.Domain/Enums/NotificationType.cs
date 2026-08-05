namespace ClinicHub.Domain.Enums
{
    public enum NotificationType
    {
        AppointmentReminder = 0,
        NewMessage = 1,
        PaymentConfirmation = 2,
        AppointmentConfirmation = 3,
        AppointmentCancellation = 4,
        SystemAnnouncement = 5,
        CancellationWindowClosed = 6,
        SubscriptionExpiring = 7,
        RefundProcessed = 8,
        AdExpiring = 9
    }
}
