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
        private static readonly Guid AdvancedPlanId = Guid.Parse("A2222222-2222-2222-2222-222222222222");

        private const string BasicFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"online_booking\",\"staff_management\",\"doctor_management\"]";

        private const string AdvancedFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"online_booking\",\"staff_management\",\"doctor_management\",\"advanced_reports\",\"marketing_tools\",\"priority_support\"]";

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
                logger.LogInformation("Plans already seeded. Nothing to insert.");
                return;
            }

            var basic = Plan.Create(BasicPlanId, "Basic", "أساسية",
                "For small clinics starting out. Up to 2 doctors and 5 staff members.",
                "للعيادات الصغيرة الجديدة. حتى 2 أطباء و 5 موظفين.",
                500, 5000, 2, 5, BasicFeatures, 1);

            var advanced = Plan.Create(AdvancedPlanId, "Advanced", "متقدمة",
                "For established clinics. Unlimited doctors and staff. All features included.",
                "للعيادات المتطورة. أطباء وموظفين غير محدودين. جميع الميزات متضمنة.",
                1500, 15000, null, null, AdvancedFeatures, 2);

            basic.MarkAsCreated("seeder");
            advanced.MarkAsCreated("seeder");

            AddPermissions(basic, SubscriptionPermission.ManageAppointments,
                SubscriptionPermission.PatientRecords,
                SubscriptionPermission.BasicReports,
                SubscriptionPermission.ManageStaff,
                SubscriptionPermission.ManageDoctors,
                SubscriptionPermission.OnlineBooking);

            AddPermissions(advanced, SubscriptionPermission.ManageAppointments,
                SubscriptionPermission.PatientRecords,
                SubscriptionPermission.BasicReports,
                SubscriptionPermission.AdvancedReports,
                SubscriptionPermission.MarketingTools,
                SubscriptionPermission.PrioritySupport,
                SubscriptionPermission.ManageStaff,
                SubscriptionPermission.ManageDoctors,
                SubscriptionPermission.OnlineBooking);

            context.Plans.AddRange(basic, advanced);
            await context.SaveChangesAsync();
            logger.LogInformation("Inserted 2 plans (Basic, Advanced) with their permissions.");
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
