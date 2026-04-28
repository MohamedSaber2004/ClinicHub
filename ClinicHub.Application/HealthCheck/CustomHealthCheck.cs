using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace ClinicHub.Application.HealthCheck
{
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly long _memoryThresholdBytes = 1024L * 1024L * 1024L;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var process = Process.GetCurrentProcess();
            var allocatedMemory = process.WorkingSet64;

            bool isHealthy = allocatedMemory <= _memoryThresholdBytes;

            var data = new Dictionary<string, object>
            {
                { "AllocatedMemory_MB", allocatedMemory / 1024 / 1024 },
                { "Threshold_MB", _memoryThresholdBytes / 1024 / 1024 },
                { "TotalThreads", process.Threads.Count }
            };

            if (isHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("System resources look good.", data: data));
            }

            return Task.FromResult(
                new HealthCheckResult(
                    context.Registration.FailureStatus,
                    "System memory usage has exceeded limits.",
                    data: data));
        }
    }
}
