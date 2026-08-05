using ClinicHub.Application.Common.Interfaces;
using Hangfire;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class BackgroundJobScheduler : IBackgroundJobScheduler
{
    public Task ScheduleAdExpirationAsync(Guid adId, DateTime endDate)
    {
        BackgroundJob.Schedule<AdExpirationJob>(job => job.MarkExpiredAsync(adId, CancellationToken.None), endDate);
        return Task.CompletedTask;
    }

    public Task ScheduleSubscriptionExpirationAsync(Guid subscriptionId, DateTime endDate)
    {
        BackgroundJob.Schedule<SubscriptionExpirationJob>(job => job.ExpireAsync(subscriptionId, CancellationToken.None), endDate);
        return Task.CompletedTask;
    }

    public Task ScheduleReservationExpirationAsync(Guid appointmentId, DateTime expiresAt)
    {
        BackgroundJob.Schedule<ReservationExpirationJob>(job => job.ExpireReservationAsync(appointmentId, CancellationToken.None), expiresAt);
        return Task.CompletedTask;
    }

    public Task ScheduleCancellationWindowCloseAsync(Guid appointmentId, DateTime windowCloseAt)
    {
        BackgroundJob.Schedule<CancellationWindowJob>(job => job.CloseCancellationWindowAsync(appointmentId, CancellationToken.None), windowCloseAt);
        return Task.CompletedTask;
    }

    public Task ScheduleNoShowMarkingAsync(Guid appointmentId, DateTime checkAt)
    {
        BackgroundJob.Schedule<NoShowJob>(job => job.MarkNoShowAsync(appointmentId, CancellationToken.None), checkAt);
        return Task.CompletedTask;
    }

    public Task ScheduleRefundRetryAsync(Guid paymentId)
    {
        BackgroundJob.Schedule<RefundRetryJob>(job => job.RetryRefundAsync(paymentId, 1, CancellationToken.None), TimeSpan.FromHours(2));
        return Task.CompletedTask;
    }
}
