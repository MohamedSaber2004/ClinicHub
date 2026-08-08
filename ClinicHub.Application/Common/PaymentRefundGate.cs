using System.Collections.Concurrent;

namespace ClinicHub.Application.Common
{
    /// <summary>
    /// Serializes refund operations per payment so a payment can never be refunded
    /// twice concurrently (double-click, duplicate webhook, cancel + retry overlap).
    /// </summary>
    public static class PaymentRefundGate
    {
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Gates = new();

        public static async Task<T> RunAsync<T>(Guid paymentId, Func<Task<T>> action)
        {
            var gate = Gates.GetOrAdd(paymentId, _ => new SemaphoreSlim(1, 1));

            await gate.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                gate.Release();
            }
        }

        public static async Task RunAsync(Guid paymentId, Func<Task> action)
        {
            var gate = Gates.GetOrAdd(paymentId, _ => new SemaphoreSlim(1, 1));

            await gate.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                gate.Release();
            }
        }
    }
}
