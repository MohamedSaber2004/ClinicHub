using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPlanFeatureKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update Basic plan features — was missing online_booking, staff_management, doctor_management
            migrationBuilder.Sql(
                "UPDATE [dbo].[Plans] SET [Features] = N'[\"appointments\",\"patient_records\",\"basic_reports\",\"online_booking\",\"staff_management\",\"doctor_management\"]' " +
                "WHERE [Id] = 'A1111111-1111-1111-1111-111111111111'");

            // Update Advanced plan features — was missing basic_reports, online_booking, staff_management, doctor_management
            migrationBuilder.Sql(
                "UPDATE [dbo].[Plans] SET [Features] = N'[\"appointments\",\"patient_records\",\"basic_reports\",\"online_booking\",\"staff_management\",\"doctor_management\",\"advanced_reports\",\"marketing_tools\",\"priority_support\"]' " +
                "WHERE [Id] = 'A2222222-2222-2222-2222-222222222222'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore Basic plan features
            migrationBuilder.Sql(
                "UPDATE [dbo].[Plans] SET [Features] = N'[\"appointments\",\"patient_records\",\"basic_reports\"]' " +
                "WHERE [Id] = 'A1111111-1111-1111-1111-111111111111'");

            // Restore Advanced plan features
            migrationBuilder.Sql(
                "UPDATE [dbo].[Plans] SET [Features] = N'[\"appointments\",\"patient_records\",\"advanced_reports\",\"marketing_tools\",\"priority_support\"]' " +
                "WHERE [Id] = 'A2222222-2222-2222-2222-222222222222'");
        }
    }
}
