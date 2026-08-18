using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStandardPlan : Migration
    {
        private const string BasicId = "'A1111111-1111-1111-1111-111111111111'";
        private const string StandardId = "'A2222222-2222-2222-2222-222222222222'";
        private const string PremiumId = "'A3333333-3333-3333-3333-333333333333'";
        private const string EnterpriseId = "'A4444444-4444-4444-4444-444444444444'";

        private const string StandardRow =
            @"('A2222222-2222-2222-2222-222222222222', 'Standard', 'قياسية', 'For growing clinics. Up to 5 doctors and 15 staff members.', 'للعيادات المتنامية. حتى 5 أطباء و 15 موظفاً.', 1000, 10000, 5, 15, '[\""appointments\"",\""patient_records\"",\""basic_reports\"",\""staff_management\"",\""doctor_management\""]', true, 2, CURRENT_TIMESTAMP, 'Migration', false)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Standard is identical to Basic (same features, same permissions) — remove it.
            // Existing Standard subscriptions are reassigned to Basic to satisfy the Restrict FK.
            migrationBuilder.Sql(
                $@"UPDATE public.""Subscriptions"" SET ""PlanId"" = {BasicId} WHERE ""PlanId"" = {StandardId}");

            migrationBuilder.Sql(
                $@"DELETE FROM public.""PlanPermissions"" WHERE ""PlanId"" = {StandardId}");

            migrationBuilder.Sql(
                $@"DELETE FROM public.""Plans"" WHERE ""Id"" = {StandardId}");

            // Re-number sort orders: Premium 3 → 2, Enterprise 4 → 3
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""SortOrder"" = 2 WHERE ""Id"" = {PremiumId}");
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""SortOrder"" = 3 WHERE ""Id"" = {EnterpriseId}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $@"INSERT INTO public.""Plans"" (""Id"", ""Name"", ""NameAr"", ""Description"", ""DescriptionAr"", ""PriceMonthly"", ""PriceYearly"", ""MaxDoctors"", ""MaxStaff"", ""Features"", ""IsActive"", ""SortOrder"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted"")
SELECT {StandardRow}
WHERE NOT EXISTS (SELECT 1 FROM public.""Plans"" WHERE ""Id"" = {StandardId})");

            // Restore Standard permissions (same set as Basic, including OnlineBooking)
            AddPermissions(migrationBuilder, StandardId, new[] { 1, 2, 4, 64, 128, 256 });

            // Note: subscriptions previously reassigned to Basic cannot be distinguished — they stay on Basic.
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""SortOrder"" = 3 WHERE ""Id"" = {PremiumId}");
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""SortOrder"" = 4 WHERE ""Id"" = {EnterpriseId}");
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
