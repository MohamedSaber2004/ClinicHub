using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlansData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
"IF NOT EXISTS (SELECT 1 FROM [dbo].[Plans]) " +
"BEGIN " +
    "INSERT INTO [dbo].[Plans] ([Id], [Name], [NameAr], [Description], [DescriptionAr], [PriceMonthly], [PriceYearly], [MaxDoctors], [MaxStaff], [Features], [IsActive], [SortOrder], [CreatedAt], [CreatedBy], [IsDeleted]) " +
    "VALUES " +
    "('A1111111-1111-1111-1111-111111111111', N'Basic', N'أساسية', N'For small clinics starting out. Up to 2 doctors and 5 staff members.', N'للعيادات الصغيرة الجديدة. حتى 2 أطباء و 5 موظفين.', 500, 5000, 2, 5, N'[\"appointments\",\"patient_records\",\"basic_reports\"]', 1, 1, GETUTCDATE(), N'Migration', 0), " +
    "('A2222222-2222-2222-2222-222222222222', N'Standard', N'قياسية', N'For growing clinics. Up to 5 doctors and 15 staff members.', N'للعيادات المتنامية. حتى 5 أطباء و 15 موظفاً.', 1000, 10000, 5, 15, N'[\"appointments\",\"patient_records\",\"advanced_reports\",\"sms_notifications\"]', 1, 2, GETUTCDATE(), N'Migration', 0), " +
    "('A3333333-3333-3333-3333-333333333333', N'Premium', N'ممتازة', N'Unlimited doctors and staff. All features included.', N'أطباء وموظفين غير محدودين. جميع الميزات متضمنة.', 2000, 20000, NULL, NULL, N'[\"appointments\",\"patient_records\",\"advanced_reports\",\"sms_notifications\",\"marketing_tools\",\"priority_support\"]', 1, 3, GETUTCDATE(), N'Migration', 0); " +
"END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
"DELETE FROM [dbo].[Plans] WHERE [Id] IN ( " +
    "'A1111111-1111-1111-1111-111111111111', " +
    "'A2222222-2222-2222-2222-222222222222', " +
    "'A3333333-3333-3333-3333-333333333333' " +
")");
        }
    }
}
