namespace ClinicHub.Application.Common.Interfaces;

public interface IBackgroundJobScheduler
{
    Task ScheduleAdExpirationAsync(Guid adId, DateTime endDate);

    Task ScheduleSubscriptionExpirationAsync(Guid subscriptionId, DateTime endDate);

    Task ScheduleReservationExpirationAsync(Guid appointmentId, DateTime expiresAt);

    Task ScheduleCancellationWindowCloseAsync(Guid appointmentId, DateTime windowCloseAt);

    Task ScheduleNoShowMarkingAsync(Guid appointmentId, DateTime checkAt);

    Task ScheduleRefundRetryAsync(Guid paymentId);
}
