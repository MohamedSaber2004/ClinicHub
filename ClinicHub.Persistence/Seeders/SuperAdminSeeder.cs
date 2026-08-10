using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Persistence.Seeders
{
    public static class SuperAdminSeeder
    {
        public static async Task SeedSuperAdminAsync(this IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<IOptions<SeedingSettings>>().Value;
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SuperAdminSeeder");

            if (!settings.Enabled)
            {
                logger.LogInformation("SuperAdmin seeding skipped (SeedingSettings.Enabled = false).");
                return;
            }

            var email = settings.SuperAdminEmail;
            if (string.IsNullOrWhiteSpace(email))
            {
                logger.LogInformation("SuperAdmin seeding skipped (SeedingSettings.SuperAdminEmail is not configured).");
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            if (await userManager.FindByEmailAsync(email) is not null)
            {
                logger.LogInformation("SuperAdmin already exists ({Email}). Nothing to create.", email);
                return;
            }

            if (!await roleManager.RoleExistsAsync(UserType.SuperAdmin.ToString()))
            {
                logger.LogWarning("SuperAdmin role does not exist yet. Run role seeding first.");
                return;
            }

            var user = ApplicationUser.Create(
                settings.SuperAdminFullName ?? "Super Admin",
                email,
                settings.SuperAdminPhoneNumber ?? string.Empty,
                null,
                null);

            var password = settings.SuperAdminPassword ?? "SuperAdmin@123";
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create SuperAdmin user: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, UserType.SuperAdmin.ToString());
            logger.LogInformation("SuperAdmin created ({Email}) with role {Role}.", email, UserType.SuperAdmin);
        }
    }
}
