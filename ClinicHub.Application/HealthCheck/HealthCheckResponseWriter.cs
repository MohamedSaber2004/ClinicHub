using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace ClinicHub.Application.HealthCheck
{
    public static class HealthCheckResponseWriter
    {
        public static Task WriteResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
                Entries = healthReport.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        Status = entry.Value.Status.ToString(),
                        entry.Value.Description,
                        Duration = entry.Value.Duration.TotalMilliseconds,
                        Error = entry.Value.Exception?.Message,
                        entry.Value.Data
                    })
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(json);
        }
    }
}
