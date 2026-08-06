using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicHub.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ClinicHubContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("CareClinicHubDb"),
                    sqlOptions =>
                    {
                        sqlOptions.UseNetTopologySuite();
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
            });

            services.AddScoped<IClinicHubContext>(provider => provider.GetRequiredService<ClinicHubContext>());

            return services;
        }
    }
}
