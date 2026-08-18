using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeOnlineBookingBaseFeature : Migration
    {
        private const string BasicId = "'A1111111-1111-1111-1111-111111111111'";
        private const string StandardId = "'A2222222-2222-2222-2222-222222222222'";
        private const string PremiumId = "'A3333333-3333-3333-3333-333333333333'";
        private const string EnterpriseId = "'A4444444-4444-4444-4444-444444444444'";

        private const string BasicFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\"]'";

        private const string StandardFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\"]'";

        private const string PremiumFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"advanced_reports\"]'";

        private const string EnterpriseFeatures =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"advanced_reports\",\"marketing_tools\"]'";

        private const string StandardFeaturesWithBooking =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\"]'";

        private const string PremiumFeaturesWithBooking =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\",\"advanced_reports\"]'";

        private const string EnterpriseFeaturesWithBooking =
            "'[\"appointments\",\"patient_records\",\"basic_reports\",\"staff_management\",\"doctor_management\",\"online_booking\",\"advanced_reports\",\"marketing_tools\"]'";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Online booking is a base feature available on all plans — remove it from the
            // differentiated features lists of Standard, Premium and Enterprise.
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""Features"" = {StandardFeatures} WHERE ""Id"" = {StandardId}");
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""Features"" = {PremiumFeatures} WHERE ""Id"" = {PremiumId}");
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""Features"" = {EnterpriseFeatures} WHERE ""Id"" = {EnterpriseId}");

            // Grant OnlineBooking (256) to Basic so the feature is truly available on all plans.
            migrationBuilder.Sql(
                $@"INSERT INTO public.""PlanPermissions"" (""Id"", ""PlanId"", ""Permission"", ""CreatedAt"", ""CreatedBy"", ""IsDeleted"", ""IsActive"")
SELECT '{Guid.NewGuid()}', {BasicId}, 256, CURRENT_TIMESTAMP, 'Migration', false, true
WHERE NOT EXISTS (SELECT 1 FROM public.""PlanPermissions"" WHERE ""PlanId"" = {BasicId} AND ""Permission"" = 256)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""Features"" = {StandardFeaturesWithBooking} WHERE ""Id"" = {StandardId}");
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""Features"" = {PremiumFeaturesWithBooking} WHERE ""Id"" = {PremiumId}");
            migrationBuilder.Sql(
                $@"UPDATE public.""Plans"" SET ""Features"" = {EnterpriseFeaturesWithBooking} WHERE ""Id"" = {EnterpriseId}");

            migrationBuilder.Sql(
                $@"DELETE FROM public.""PlanPermissions"" WHERE ""PlanId"" = {BasicId} AND ""Permission"" = 256");
        }
    }
}
