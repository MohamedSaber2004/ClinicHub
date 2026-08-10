using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReduceToTwoPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove Premium plan permissions
            migrationBuilder.Sql("DELETE FROM [dbo].[PlanPermissions] WHERE [PlanId] = 'A3333333-3333-3333-3333-333333333333'");

            // Reassign any subscriptions on Premium to Advanced
            migrationBuilder.Sql("UPDATE [dbo].[Subscriptions] SET [PlanId] = 'A2222222-2222-2222-2222-222222222222' WHERE [PlanId] = 'A3333333-3333-3333-3333-333333333333'");

            // Remove Premium plan
            migrationBuilder.Sql("DELETE FROM [dbo].[Plans] WHERE [Id] = 'A3333333-3333-3333-3333-333333333333'");

            // Rename Standard → Advanced
            migrationBuilder.Sql("UPDATE [dbo].[Plans] SET [Name] = N'Advanced', [NameAr] = N'متقدمة', [Description] = N'For established clinics. Unlimited doctors and staff. All features included.', [DescriptionAr] = N'للعيادات المتطورة. أطباء وموظفين غير محدودين. جميع الميزات متضمنة.', [PriceMonthly] = 1500, [PriceYearly] = 15000, [MaxDoctors] = NULL, [MaxStaff] = NULL, [SortOrder] = 2 WHERE [Id] = 'A2222222-2222-2222-2222-222222222222'");

            // Remove AdvancedReports permission from Basic (keep only core)
            migrationBuilder.Sql("DELETE FROM [dbo].[PlanPermissions] WHERE [PlanId] = 'A1111111-1111-1111-1111-111111111111' AND [Permission] = 8");

            // Update Basic permissions: keep ManageAppointments(1), PatientRecords(2), BasicReports(4), ManageStaff(64), ManageDoctors(128), OnlineBooking(256)
            // Update Advanced permissions: Basic + AdvancedReports(8), MarketingTools(16), PrioritySupport(32)
            // (Remove any extras if needed)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore Premium plan
            migrationBuilder.Sql(
"IF NOT EXISTS (SELECT 1 FROM [dbo].[Plans] WHERE [Id] = 'A3333333-3333-3333-3333-333333333333') " +
"INSERT INTO [dbo].[Plans] ([Id], [Name], [NameAr], [Description], [DescriptionAr], [PriceMonthly], [PriceYearly], [MaxDoctors], [MaxStaff], [Features], [IsActive], [SortOrder], [CreatedAt], [CreatedBy], [IsDeleted]) " +
"VALUES ('A3333333-3333-3333-3333-333333333333', N'Premium', N'ممتازة', N'Unlimited doctors and staff. All features included.', N'أطباء وموظفين غير محدودين. جميع الميزات متضمنة.', 2000, 20000, NULL, NULL, N'[\"appointments\",\"patient_records\",\"advanced_reports\",\"sms_notifications\",\"marketing_tools\",\"priority_support\"]', 1, 3, GETUTCDATE(), N'Migration', 0)");

            // Restore Premium permissions
            var premiumPerms = new[] { 1, 2, 4, 8, 16, 32, 64, 128, 256 };
            foreach (var perm in premiumPerms)
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[PlanPermissions] WHERE [PlanId] = 'A3333333-3333-3333-3333-333333333333' AND [Permission] = {perm})
    INSERT INTO [dbo].[PlanPermissions] ([Id], [PlanId], [Permission], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive])
    VALUES (NEWID(), 'A3333333-3333-3333-3333-333333333333', {perm}, GETUTCDATE(), N'Migration', 0, 1);
");
            }

            // Revert Advanced → Standard
            migrationBuilder.Sql("UPDATE [dbo].[Plans] SET [Name] = N'Standard', [NameAr] = N'قياسية', [Description] = N'For growing clinics. Up to 5 doctors and 15 staff members.', [DescriptionAr] = N'للعيادات المتنامية. حتى 5 أطباء و 15 موظفاً.', [PriceMonthly] = 1000, [PriceYearly] = 10000, [MaxDoctors] = 5, [MaxStaff] = 15, [SortOrder] = 2 WHERE [Id] = 'A2222222-2222-2222-2222-222222222222'");

            // Restore AdvancedReports to Basic
            migrationBuilder.Sql(
"IF NOT EXISTS (SELECT 1 FROM [dbo].[PlanPermissions] WHERE [PlanId] = 'A1111111-1111-1111-1111-111111111111' AND [Permission] = 8) " +
"INSERT INTO [dbo].[PlanPermissions] ([Id], [PlanId], [Permission], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive]) " +
"VALUES (NEWID(), 'A1111111-1111-1111-1111-111111111111', 8, GETUTCDATE(), N'Migration', 0, 1)");
        }
    }
}
