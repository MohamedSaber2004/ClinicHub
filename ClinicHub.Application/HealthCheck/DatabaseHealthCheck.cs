using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

namespace ClinicHub.Application.HealthCheck
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public DatabaseHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    return HealthCheckResult.Healthy(
                        description: "Database connection successful. ClinicHub database is responding correctly."
                    );
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    description: $"Database connection failed: {ex.Message}",
                    exception: ex
                );
            }
        }
    }
}
