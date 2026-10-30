using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ClinicHub.Persistence
{
    public sealed class ClinicHubContextFactory : IDesignTimeDbContextFactory<ClinicHubContext>
    {
        public ClinicHubContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var candidates = new[]
            {
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "ClinicHub.API")),
                Directory.GetCurrentDirectory()
            };
            var basePath = candidates.First(Directory.Exists);

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("CareClinicHubDb")
                ?? config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    $"No 'CareClinicHubDb' or 'DefaultConnection' connection string found for environment '{env}' (basePath: {basePath}).");

            var options = new DbContextOptionsBuilder<ClinicHubContext>()
                .UseSqlServer(connectionString, x => x.UseNetTopologySuite())
                .Options;

            return new ClinicHubContext(null, options);
        }
    }
}
