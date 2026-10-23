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

            // No hardcoded credentials: production seeding is disabled by default
            // (see scripts/prod-seed). Dev/Test provide values via appsettings or
            // SeedingSettings__SuperAdminPassword environment variable.
            if (string.IsNullOrWhiteSpace(settings.SuperAdminEmail) ||
                string.IsNullOrWhiteSpace(settings.SuperAdminPassword))
            {
                logger.LogInformation("SuperAdmin seeding skipped (SuperAdminEmail/Password not configured).");
                return;
            }

            var email = settings.SuperAdminEmail.Trim();
            var password = settings.SuperAdminPassword;

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            var roleName = UserType.SuperAdmin.ToString();
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Failed to create SuperAdmin role: {Errors}", string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                    return;
                }
            }

            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, roleName))
                {
                    await userManager.AddToRoleAsync(existing, roleName);
                    logger.LogInformation("SuperAdmin already exists ({Email}). Added missing role {Role}.", email, roleName);
                }
                else
                {
                    logger.LogInformation("SuperAdmin already exists ({Email}). Nothing to create.", email);
                }

                if (!existing.EmailConfirmed || !existing.IsActive || existing.IsDeleted)
                {
                    existing.EmailConfirmed = true;
                    existing.IsActive = true;
                    existing.IsDeleted = false;
                    await userManager.UpdateAsync(existing);
                }
                return;
            }

            var user = ApplicationUser.Create(
                string.IsNullOrWhiteSpace(settings.SuperAdminFullName) ? "Super Admin" : settings.SuperAdminFullName!,
                email,
                settings.SuperAdminPhoneNumber ?? string.Empty,
                null,
                null);

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create SuperAdmin user: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, roleName);
            logger.LogInformation("SuperAdmin created ({Email}) with role {Role}.", email, UserType.SuperAdmin);
        }
    }
}
