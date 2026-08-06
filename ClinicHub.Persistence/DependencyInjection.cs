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
                    x => x.UseNetTopologySuite());
            });

            services.AddScoped<IClinicHubContext>(provider => provider.GetRequiredService<ClinicHubContext>());

            return services;
        }
    }
}
