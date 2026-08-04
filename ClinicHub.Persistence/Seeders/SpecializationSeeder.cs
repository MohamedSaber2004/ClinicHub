using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ClinicHub.Persistence.Seeders
{
    public static class SpecializationSeeder
    {
        public static async Task SeedSpecializationsAsync(this IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<IOptions<SeedingSettings>>().Value;
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SpecializationSeeder");

            if (!settings.Enabled)
            {
                logger.LogInformation("Specialization seeding skipped (SeedingSettings.Enabled = false).");
                return;
            }

            var context = serviceProvider.GetRequiredService<ClinicHubContext>();

            var jsonPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "specializations.json");
            if (!File.Exists(jsonPath))
            {
                logger.LogWarning("Specialization seed file not found at {Path}. Skipping.", jsonPath);
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var items = JsonSerializer.Deserialize<List<SpecializationSeedItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (items is null || items.Count == 0)
            {
                logger.LogWarning("Specialization seed file contains no items. Skipping.");
                return;
            }

            var existingNames = await context.Specializations
                .IgnoreQueryFilters()
                .Select(x => x.Name)
                .ToListAsync();

            var toInsert = items.Where(item => !existingNames.Contains(item.Name)).ToList();
            if (toInsert.Count == 0)
            {
                logger.LogInformation("Specializations already seeded ({Count} items). Nothing to insert.", existingNames.Count);
                return;
            }

            foreach (var item in toInsert)
            {
                var specialization = new Specialization
                {
                    Name = item.Name,
                    ArName = item.ArName,
                    Description = item.Description,
                    IconUrl = item.IconUrl,
                    IsFamous = item.IsFamous
                };
                specialization.MarkAsCreated("seeder");

                context.Specializations.Add(specialization);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Inserted {Count} specializations.", toInsert.Count);
        }

        private class SpecializationSeedItem
        {
            public string Name { get; set; } = null!;
            public string ArName { get; set; } = null!;
            public string? Description { get; set; }
            public string? IconUrl { get; set; }
            public bool IsFamous { get; set; }
        }
    }
}
