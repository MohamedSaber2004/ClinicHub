using System.Text.Json;
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Persistence.Seeders
{
    public static class SpecializationSeeder
    {
        public static async Task SeedSpecializationsAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ClinicHubContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SpecializationSeeder");

            if (await context.Specializations.AnyAsync())
            {
                return;
            }

            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "specializations.json");
                if (!File.Exists(path))
                {
                    // Fallback to searching in the project directory if not found in BaseDirectory (useful for some dev environments)
                    var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ClinicHub.Persistence", "SeedData", "specializations.json");
                    if (File.Exists(projectPath))
                    {
                        path = projectPath;
                    }
                    else
                    {
                        logger.LogWarning("Specialization seed file not found at {Path}", path);
                        return;
                    }
                }

                var json = await File.ReadAllTextAsync(path);
                var specializations = JsonSerializer.Deserialize<List<Specialization>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (specializations != null && specializations.Any())
                {
                    context.Specializations.AddRange(specializations);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Successfully seeded {Count} specializations.", specializations.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding specializations.");
            }
        }
    }
}
