using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAdvancedPlanPermissions : Migration
    {
        private const string AdvancedPlanId = "'A2222222-2222-2222-2222-222222222222'";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MarketingTools = 16
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[PlanPermissions] WHERE [PlanId] = {AdvancedPlanId} AND [Permission] = 16)
    INSERT INTO [dbo].[PlanPermissions] ([Id], [PlanId], [Permission], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive])
    VALUES (NEWID(), {AdvancedPlanId}, 16, GETUTCDATE(), N'Migration', 0, 1);");

            // PrioritySupport = 32
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[PlanPermissions] WHERE [PlanId] = {AdvancedPlanId} AND [Permission] = 32)
    INSERT INTO [dbo].[PlanPermissions] ([Id], [PlanId], [Permission], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive])
    VALUES (NEWID(), {AdvancedPlanId}, 32, GETUTCDATE(), N'Migration', 0, 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove MarketingTools = 16
            migrationBuilder.Sql($"DELETE FROM [dbo].[PlanPermissions] WHERE [PlanId] = {AdvancedPlanId} AND [Permission] = 16");

            // Remove PrioritySupport = 32
            migrationBuilder.Sql($"DELETE FROM [dbo].[PlanPermissions] WHERE [PlanId] = {AdvancedPlanId} AND [Permission] = 32");
        }
    }
}
