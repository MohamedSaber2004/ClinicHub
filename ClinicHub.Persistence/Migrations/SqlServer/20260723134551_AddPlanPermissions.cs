using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanPermissions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanPermissions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "dbo",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Basic plan — core features only
            var basicPerms = new[] { 1, 2, 4, 64, 128, 256 };
            foreach (var perm in basicPerms)
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[PlanPermissions] WHERE [PlanId] = 'A1111111-1111-1111-1111-111111111111' AND [Permission] = {perm})
    INSERT INTO [dbo].[PlanPermissions] ([Id], [PlanId], [Permission], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive])
    VALUES (NEWID(), 'A1111111-1111-1111-1111-111111111111', {perm}, GETUTCDATE(), N'Migration', 0, 1);
");
            }

            // Standard plan — adds AdvancedReports (8)
            var standardPerms = new[] { 1, 2, 4, 8, 64, 128, 256 };
            foreach (var perm in standardPerms)
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[PlanPermissions] WHERE [PlanId] = 'A2222222-2222-2222-2222-222222222222' AND [Permission] = {perm})
    INSERT INTO [dbo].[PlanPermissions] ([Id], [PlanId], [Permission], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive])
    VALUES (NEWID(), 'A2222222-2222-2222-2222-222222222222', {perm}, GETUTCDATE(), N'Migration', 0, 1);
");
            }

            // Premium plan — all permissions
            var premiumPerms = new[] { 1, 2, 4, 8, 16, 32, 64, 128, 256 };
            foreach (var perm in premiumPerms)
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[PlanPermissions] WHERE [PlanId] = 'A3333333-3333-3333-3333-333333333333' AND [Permission] = {perm})
    INSERT INTO [dbo].[PlanPermissions] ([Id], [PlanId], [Permission], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive])
    VALUES (NEWID(), 'A3333333-3333-3333-3333-333333333333', {perm}, GETUTCDATE(), N'Migration', 0, 1);
");
            }

            migrationBuilder.CreateIndex(
                name: "IX_PlanPermissions_PlanId_Permission",
                schema: "dbo",
                table: "PlanPermissions",
                columns: new[] { "PlanId", "Permission" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanPermissions",
                schema: "dbo");
        }
    }
}
