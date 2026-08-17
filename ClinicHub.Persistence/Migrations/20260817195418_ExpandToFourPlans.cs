using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandToFourPlans : Migration
    {
        private const string BasicId = "'A1111111-1111-1111-1111-111111111111'";
        private const string StandardId = "'A2222222-2222-2222-2222-222222222222'";
        private const string PremiumId = "'A3333333-3333-3333-3333-333333333333'";
        private const string EnterpriseId = "'A4444444-4444-4444-4444-444444444444'";

        private const string BasicFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\"]'";

        private const string StandardFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\"]'";

        private const string PremiumFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\",\"advanced_reports\"]'";

        private const string EnterpriseFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\",\"advanced_reports\",\"marketing_tools\"]'";

        private const string AdvancedFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"online_booking\",\"staff_management\",\"doctor_management\",\"advanced_reports\",\"marketing_tools\",\"priority_support\"]'";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PrioritySupport (32) is not implemented — remove it from all plans
            migrationBuilder.Sql("DELETE FROM public.\"PlanPermissions\" WHERE \"Permission\" = 32");

            // Reset permissions of the two existing plans (new tiering applied below)
            migrationBuilder.Sql($"DELETE FROM public.\"PlanPermissions\" WHERE \"PlanId\" IN ({BasicId}, {StandardId})");

            // Basic: core features only (OnlineBooking moved to Standard)
            migrationBuilder.Sql(
$@"UPDATE public.""Plans"" SET
    ""Description"" = 'For small clinics starting out. Up to 2 doctors and 5 staff members.',
    ""DescriptionAr"" = 'للعيادات الصغيرة الجديدة. حتى 2 أطباء و 5 موظفين.',
    ""PriceMonthly"" = 500, ""PriceYearly"" = 5000,
    ""MaxDoctors"" = 2, ""MaxStaff"" = 5,
    ""Features"" = {BasicFeatures},
    ""SortOrder"" = 1
WHERE ""Id"" = {BasicId}");

            // Rename Advanced → Standard (adds OnlineBooking)
            migrationBuilder.Sql(
$@"UPDATE public.""Plans"" SET
    ""Name"" = 'Standard', ""NameAr"" = 'قياسية',
    ""Description"" = 'For growing clinics. Up to 5 doctors and 15 staff members.',
    ""DescriptionAr"" = 'للعيادات المتنامية. حتى 5 أطباء و 15 موظفاً.',
    ""PriceMonthly"" = 1000, ""PriceYearly"" = 10000,
    ""MaxDoctors"" = 5, ""MaxStaff"" = 15,
    ""Features"" = {StandardFeatures},
    ""SortOrder"" = 2
WHERE ""Id"" = {StandardId}");

            // Premium: adds AdvancedReports
            migrationBuilder.Sql(
$@"INSERT INTO public.""Plans"" (""Id"", ""Name"", ""NameAr"", ""Description"", ""DescriptionAr"", ""PriceMonthly"", ""PriceYearly"", ""MaxDoctors"", ""MaxStaff"", ""Features"", ""IsActive"", ""SortOrder"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted"")
SELECT {PremiumId}, 'Premium', 'ممتازة', 'For advanced clinics. Up to 10 doctors and 30 staff members.', 'للعيادات المتقدمة. حتى 10 أطباء و 30 موظفاً.', 1500, 15000, 10, 30, {PremiumFeatures}, true, 3, CURRENT_TIMESTAMP, 'Migration', false
WHERE NOT EXISTS (SELECT 1 FROM public.""Plans"" WHERE ""Id"" = {PremiumId})");

            // Enterprise: adds MarketingTools
            migrationBuilder.Sql(
$@"INSERT INTO public.""Plans"" (""Id"", ""Name"", ""NameAr"", ""Description"", ""DescriptionAr"", ""PriceMonthly"", ""PriceYearly"", ""MaxDoctors"", ""MaxStaff"", ""Features"", ""IsActive"", ""SortOrder"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted"")
SELECT {EnterpriseId}, 'Enterprise', 'المؤسسات', 'For large clinics. Unlimited doctors and staff. All features included.', 'للعيادات الكبيرة. أطباء وموظفين غير محدودين. جميع الميزات متضمنة.', 2500, 25000, NULL, NULL, {EnterpriseFeatures}, true, 4, CURRENT_TIMESTAMP, 'Migration', false
WHERE NOT EXISTS (SELECT 1 FROM public.""Plans"" WHERE ""Id"" = {EnterpriseId})");

            // Permissions per tier (each plan adds exactly one feature over the previous)
            AddPermissions(migrationBuilder, BasicId, new[] { 1, 2, 4, 64, 128 });
            AddPermissions(migrationBuilder, StandardId, new[] { 1, 2, 4, 64, 128, 256 });
            AddPermissions(migrationBuilder, PremiumId, new[] { 1, 2, 4, 64, 128, 256, 8 });
            AddPermissions(migrationBuilder, EnterpriseId, new[] { 1, 2, 4, 64, 128, 256, 8, 16 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Premium & Enterprise plans
            migrationBuilder.Sql($"DELETE FROM public.\"PlanPermissions\" WHERE \"PlanId\" IN ({PremiumId}, {EnterpriseId})");
            migrationBuilder.Sql($"DELETE FROM public.\"Plans\" WHERE \"Id\" IN ({PremiumId}, {EnterpriseId})");

            // Restore Standard → Advanced
            migrationBuilder.Sql(
$@"UPDATE public.""Plans"" SET
    ""Name"" = 'Advanced', ""NameAr"" = 'متقدمة',
    ""Description"" = 'For established clinics. Unlimited doctors and staff. All features included.',
    ""DescriptionAr"" = 'للعيادات المتطورة. أطباء وموظفين غير محدودين. جميع الميزات متضمنة.',
    ""PriceMonthly"" = 1500, ""PriceYearly"" = 15000,
    ""MaxDoctors"" = NULL, ""MaxStaff"" = NULL,
    ""Features"" = {AdvancedFeatures},
    ""SortOrder"" = 2
WHERE ""Id"" = {StandardId}");

            // Restore Basic features (OnlineBooking included)
            migrationBuilder.Sql(
$@"UPDATE public.""Plans"" SET ""Features"" = '[\""appointments\"",\""patient_records\"",\""basic_reports\"",\""online_booking\"",\""staff_management\"",\""doctor_management\""]' WHERE ""Id"" = {BasicId}");

            // Restore permissions: Basic gets OnlineBooking (256), Advanced gets AdvancedReports(8), MarketingTools(16), PrioritySupport(32)
            AddPermissions(migrationBuilder, BasicId, new[] { 256 });
            AddPermissions(migrationBuilder, StandardId, new[] { 8, 16, 32 });
        }

        private static void AddPermissions(MigrationBuilder migrationBuilder, string planId, int[] permissions)
        {
            foreach (var permission in permissions)
            {
                migrationBuilder.Sql(
$@"INSERT INTO public.""PlanPermissions"" (""Id"", ""PlanId"", ""Permission"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted"", ""IsActive"")
SELECT '{Guid.NewGuid()}', {planId}, {permission}, CURRENT_TIMESTAMP, 'Migration', false, true
WHERE NOT EXISTS (SELECT 1 FROM public.""PlanPermissions"" WHERE ""PlanId"" = {planId} AND ""Permission"" = {permission})");
            }
        }
    }
}