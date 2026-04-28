using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClinicHub.Application.HealthCheck
{
    public class DiskSpaceHealthCheck : IHealthCheck
    {
        private readonly string _drive;
        private readonly long _minimumFreeMB;

        public DiskSpaceHealthCheck(string drive, long minimumFreeMB = 1024)
        {
            _drive = drive;
            _minimumFreeMB = minimumFreeMB;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var driveInfo = new DriveInfo(_drive);
                var freeSpaceMB = driveInfo.AvailableFreeSpace / (1024 * 1024);

                if (freeSpaceMB >= _minimumFreeMB)
                {
                    return Task.FromResult(HealthCheckResult.Healthy(
                        description: $"Disk space check passed. Available: {freeSpaceMB} MB on {_drive} drive"
                    ));
                }

                return Task.FromResult(HealthCheckResult.Unhealthy(
                    description: $"Insufficient disk space. Available: {freeSpaceMB} MB on {_drive} drive. Required: {_minimumFreeMB} MB"
                ));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    description: $"Failed to check disk space: {ex.Message}",
                    exception: ex
                ));
            }
        }
    }
}
