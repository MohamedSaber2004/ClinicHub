#pragma warning disable CS0618 // MarketingTools is obsolete (ads independent) — needed only to clean legacy data
using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Persistence.Seeders
{
    public static class PlanSeeder
    {
        private static readonly Guid BasicPlanId = Guid.Parse("A1111111-1111-1111-1111-111111111111");
        private static readonly Guid PremiumPlanId = Guid.Parse("A3333333-3333-3333-3333-333333333333");
        private static readonly Guid EnterprisePlanId = Guid.Parse("A4444444-4444-4444-4444-444444444444");

        // ADS IS NOW INDEPENDENT — marketing_tools removed from all plans.
        // Two plans only: Basic (limited) + Premium (unlimited, full features).
        private const string BasicFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\"]";

        private const string PremiumFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\",\"advanced_reports\"]";

        public static async Task SeedPlansAsync(this IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetRequiredService<IOptions<SeedingSettings>>().Value;
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PlanSeeder");

            if (!settings.Enabled)
            {
                logger.LogInformation("Plan seeding skipped (SeedingSettings.Enabled = false).");
                return;
            }

            var context = serviceProvider.GetRequiredService<ClinicHubContext>();

            if (await context.Plans.IgnoreQueryFilters().AnyAsync())
            {
                // Synchronize existing data to 2-plan model + ads independence
                await SynchronizeToTwoPlansAsync(context, logger);
                return;
            }

            // Fresh DB — create exactly 2 plans
            var basic = Plan.Create(BasicPlanId, "Basic", "أساسية",
                "For small clinics starting out. Up to 2 doctors and 5 staff members.",
                "للعيادات الصغيرة الجديدة. حتى 2 أطباء و 5 موظفين.",
                500, 5000, 2, 5, BasicFeatures, 1);

            var premium = Plan.Create(PremiumPlanId, "Premium", "ممتازة",
                "For medium/large clinics. Unlimited doctors and staff. All features included.",
                "للعيادات المتوسطة والكبيرة. أطباء وموظفون غير محدودين. جميع الميزات متضمنة.",
                2500, 25000, null, null, PremiumFeatures, 2);

            basic.MarkAsCreated("seeder");
            premium.MarkAsCreated("seeder");

            AddPermissions(basic, SubscriptionPermission.ManageAppointments,
                SubscriptionPermission.PatientRecords,
                SubscriptionPermission.BasicReports,
                SubscriptionPermission.ManageStaff,
                SubscriptionPermission.ManageDoctors,
                SubscriptionPermission.OnlineBooking);

            AddPermissions(premium, SubscriptionPermission.ManageAppointments,
                SubscriptionPermission.PatientRecords,
                SubscriptionPermission.BasicReports,
                SubscriptionPermission.ManageStaff,
                SubscriptionPermission.ManageDoctors,
                SubscriptionPermission.OnlineBooking,
                SubscriptionPermission.AdvancedReports);

            context.Plans.AddRange(basic, premium);
            await context.SaveChangesAsync();
            logger.LogInformation("Inserted 2 plans (Basic, Premium) with their permissions. Ads is independent (no marketing_tools).");
            return;
        }

        private static async Task SynchronizeToTwoPlansAsync(ClinicHubContext context, ILogger logger)
        {
            var plans = await context.Plans.IgnoreQueryFilters().Include(p => p.Permissions).ToListAsync();
            var hasChanges = false;

            // 1. Strip marketing_tools from Features JSON and Permissions (ads independence)
            foreach (var plan in plans)
            {
                if (!string.IsNullOrWhiteSpace(plan.Features) && plan.Features.Contains("marketing_tools"))
                {
                    // Remove marketing_tools key from JSON array string
                    var cleaned = plan.Features.Replace("\"marketing_tools\",", "")
                                               .Replace(",\"marketing_tools\"", "")
                                               .Replace("\"marketing_tools\"", "")
                                               .Replace(",,", ",")
                                               .Replace("[,", "[")
                                               .Replace(",]", "]");
                    plan.Features = cleaned;
                    hasChanges = true;
                }

                var toRemove = plan.Permissions.Where(pp => pp.Permission == SubscriptionPermission.MarketingTools).ToList();
                foreach (var r in toRemove)
                {
                    context.Set<PlanPermission>().Remove(r);
                    hasChanges = true;
                }
            }

            // 2. Enforce exactly 2 active plans: Basic + Premium. Enterprise (A444...) is retired.
            var enterprise = plans.FirstOrDefault(p => p.Id == EnterprisePlanId);
            if (enterprise != null)
            {
                // Reassign subscriptions from Enterprise to Premium before removing
                var subsToReassign = await context.Set<Subscription>()
                    .IgnoreQueryFilters()
                    .Where(s => s.PlanId == EnterprisePlanId)
                    .ToListAsync();
                foreach (var sub in subsToReassign)
                {
                    sub.PlanId = PremiumPlanId;
                    hasChanges = true;
                }

                // Remove Enterprise permissions already handled above, then delete plan
                var perms = await context.Set<PlanPermission>().IgnoreQueryFilters()
                    .Where(pp => pp.PlanId == EnterprisePlanId).ToListAsync();
                context.Set<PlanPermission>().RemoveRange(perms);
                context.Plans.Remove(enterprise);
                hasChanges = true;
                logger.LogInformation("Removed Enterprise plan (A444...). Reassigned {Count} subscriptions to Premium.", subsToReassign.Count);
            }

            // 3. Ensure Basic and Premium have correct Features / limits / sort order
            var basic = plans.FirstOrDefault(p => p.Id == BasicPlanId);
            if (basic != null)
            {
                if (basic.Features != BasicFeatures) { basic.Features = BasicFeatures; hasChanges = true; }
                if (basic.MaxDoctors != 2 || basic.MaxStaff != 5) { basic.MaxDoctors = 2; basic.MaxStaff = 5; hasChanges = true; }
                if (basic.SortOrder != 1) { basic.SortOrder = 1; hasChanges = true; }
                if (!basic.IsActive) { basic.IsActive = true; hasChanges = true; }
                // Ensure Basic permissions are exactly the expected set
                await SyncPermissionsAsync(context, basic, new[]
                {
                    SubscriptionPermission.ManageAppointments,
                    SubscriptionPermission.PatientRecords,
                    SubscriptionPermission.BasicReports,
                    SubscriptionPermission.ManageStaff,
                    SubscriptionPermission.ManageDoctors,
                    SubscriptionPermission.OnlineBooking
                });
            }

            var premium = plans.FirstOrDefault(p => p.Id == PremiumPlanId);
            if (premium != null)
            {
                if (premium.Features != PremiumFeatures) { premium.Features = PremiumFeatures; hasChanges = true; }
                // Upgrade Premium to unlimited (was 10/30) to absorb Enterprise tier
                if (premium.MaxDoctors != null || premium.MaxStaff != null) { premium.MaxDoctors = null; premium.MaxStaff = null; hasChanges = true; }
                if (premium.PriceMonthly != 2500 || premium.PriceYearly != 25000) { premium.PriceMonthly = 2500; premium.PriceYearly = 25000; hasChanges = true; }
                if (premium.SortOrder != 2) { premium.SortOrder = 2; hasChanges = true; }
                if (!premium.IsActive) { premium.IsActive = true; hasChanges = true; }
                if (premium.Name != "Premium") { premium.Name = "Premium"; hasChanges = true; }
                if (premium.NameAr != "ممتازة") { premium.NameAr = "ممتازة"; hasChanges = true; }
                await SyncPermissionsAsync(context, premium, new[]
                {
                    SubscriptionPermission.ManageAppointments,
                    SubscriptionPermission.PatientRecords,
                    SubscriptionPermission.BasicReports,
                    SubscriptionPermission.ManageStaff,
                    SubscriptionPermission.ManageDoctors,
                    SubscriptionPermission.OnlineBooking,
                    SubscriptionPermission.AdvancedReports
                });
            }

            // 4. Deactivate any other unexpected plans (keep only the two)
            foreach (var extra in plans.Where(p => p.Id != BasicPlanId && p.Id != PremiumPlanId && p.Id != EnterprisePlanId))
            {
                if (extra.IsActive) { extra.IsActive = false; hasChanges = true; }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Synchronized plans to 2-plan model (Basic + Premium). Ads is now independent.");
            }
            else
            {
                logger.LogInformation("Plans already in sync (2 plans, no marketing_tools). Nothing to update.");
            }
        }

        private static async Task SyncPermissionsAsync(ClinicHubContext context, Plan plan, SubscriptionPermission[] desired)
        {
            var desiredSet = desired.ToHashSet();
            var current = plan.Permissions.ToList();
            foreach (var cur in current.Where(c => !desiredSet.Contains(c.Permission)).ToList())
            {
                context.Set<PlanPermission>().Remove(cur);
            }
            foreach (var perm in desired.Where(d => !current.Any(c => c.Permission == d)))
            {
                var pp = new PlanPermission { PlanId = plan.Id, Permission = perm };
                pp.MarkAsCreated("seeder-sync");
                plan.Permissions.Add(pp);
            }
        }

        private static void AddPermissions(Plan plan, params SubscriptionPermission[] permissions)
        {
            foreach (var permission in permissions)
            {
                var planPermission = new PlanPermission
                {
                    PlanId = plan.Id,
                    Permission = permission
                };
                planPermission.MarkAsCreated("seeder");
                plan.Permissions.Add(planPermission);
            }
        }
    }
}
