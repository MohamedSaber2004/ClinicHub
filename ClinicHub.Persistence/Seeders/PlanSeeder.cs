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
        private static readonly Guid StandardPlanId = Guid.Parse("A2222222-2222-2222-2222-222222222222");
        private static readonly Guid PremiumPlanId = Guid.Parse("A3333333-3333-3333-3333-333333333333");
        private static readonly Guid EnterprisePlanId = Guid.Parse("A4444444-4444-4444-4444-444444444444");

        private const string BasicFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\"]";

        private const string StandardFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\"]";

        private const string PremiumFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\",\"advanced_reports\"]";

        private const string EnterpriseFeatures =
            "[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\",\"advanced_reports\",\"marketing_tools\"]";

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

            var standard = Plan.Create(StandardPlanId, "Standard", "قياسية",
                "For growing clinics. Up to 5 doctors and 15 staff members.",
                "للعيادات المتنامية. حتى 5 أطباء و 15 موظفاً.",
                1000, 10000, 5, 15, StandardFeatures, 2);

            var premium = Plan.Create(PremiumPlanId, "Premium", "ممتازة",
                "For advanced clinics. Up to 10 doctors and 30 staff members.",
                "للعيادات المتقدمة. حتى 10 أطباء و 30 موظفاً.",
                1500, 15000, 10, 30, PremiumFeatures, 3);

            var enterprise = Plan.Create(EnterprisePlanId, "Enterprise", "المؤسسات",
                "For large clinics. Unlimited doctors and staff. All features included.",
                "للعيادات الكبيرة. أطباء وموظفين غير محدودين. جميع الميزات متضمنة.",
                2500, 25000, null, null, EnterpriseFeatures, 4);

            basic.MarkAsCreated("seeder");
            standard.MarkAsCreated("seeder");
            premium.MarkAsCreated("seeder");
            enterprise.MarkAsCreated("seeder");

            AddPermissions(basic, SubscriptionPermission.ManageAppointments,
                SubscriptionPermission.PatientRecords,
                SubscriptionPermission.BasicReports,
                SubscriptionPermission.ManageStaff,
                SubscriptionPermission.ManageDoctors);

            AddPermissions(standard, SubscriptionPermission.ManageAppointments,
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

            AddPermissions(enterprise, SubscriptionPermission.ManageAppointments,
                SubscriptionPermission.PatientRecords,
                SubscriptionPermission.BasicReports,
                SubscriptionPermission.ManageStaff,
                SubscriptionPermission.ManageDoctors,
                SubscriptionPermission.OnlineBooking,
                SubscriptionPermission.AdvancedReports,
                SubscriptionPermission.MarketingTools);

            context.Plans.AddRange(basic, standard, premium, enterprise);
            await context.SaveChangesAsync();
            logger.LogInformation("Inserted 4 plans (Basic, Standard, Premium, Enterprise) with their permissions.");
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
