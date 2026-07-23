using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Clinics_ClinicId1",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_ClinicId1",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "Plan",
                schema: "dbo",
                table: "Subscriptions",
                newName: "Period");

            migrationBuilder.RenameColumn(
                name: "ClinicId1",
                schema: "dbo",
                table: "Subscriptions",
                newName: "PaymentId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                schema: "dbo",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                schema: "dbo",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                schema: "dbo",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                schema: "dbo",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPeriod",
                schema: "dbo",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Plans",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PriceMonthly = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceYearly = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDoctors = table.Column<int>(type: "int", nullable: true),
                    MaxStaff = table.Column<int>(type: "int", nullable: true),
                    Features = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PaymentId",
                schema: "dbo",
                table: "Subscriptions",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                schema: "dbo",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                schema: "dbo",
                table: "Payments",
                column: "SubscriptionId",
                unique: true,
                filter: "[SubscriptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_SortOrder",
                schema: "dbo",
                table: "Plans",
                column: "SortOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Payments_PaymentId",
                schema: "dbo",
                table: "Subscriptions",
                column: "PaymentId",
                principalSchema: "dbo",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Plans_PlanId",
                schema: "dbo",
                table: "Subscriptions",
                column: "PlanId",
                principalSchema: "dbo",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Payments_PaymentId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Plans_PlanId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Plans",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PaymentId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PlanId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SubscriptionId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PlanId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PlanId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SubscriptionPeriod",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Period",
                schema: "dbo",
                table: "Subscriptions",
                newName: "Plan");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                schema: "dbo",
                table: "Subscriptions",
                newName: "ClinicId1");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                schema: "dbo",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ClinicId1",
                schema: "dbo",
                table: "Subscriptions",
                column: "ClinicId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Clinics_ClinicId1",
                schema: "dbo",
                table: "Subscriptions",
                column: "ClinicId1",
                principalSchema: "dbo",
                principalTable: "Clinics",
                principalColumn: "Id");
        }
    }
}
