using ClinicHub.Application.Common.Models;
using ClinicHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClinicHub.Persistence.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(this IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<IOptions<SeedingSettings>>().Value;
            if (!settings.Enabled) return;

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            
            foreach (var roleName in Enum.GetNames<UserType>())
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }
        }
    }
}
