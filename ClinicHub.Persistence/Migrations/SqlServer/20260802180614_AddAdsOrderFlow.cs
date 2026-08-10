using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdsOrderFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "AdPackageId",
                schema: "dbo",
                table: "Advertisements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                schema: "dbo",
                table: "Advertisements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                schema: "dbo",
                table: "Advertisements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_AdPackageId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_ClinicId_Status",
                schema: "dbo",
                table: "Advertisements",
                columns: new[] { "ClinicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_PaymentId",
                schema: "dbo",
                table: "Advertisements",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_AdPackages_AdPackageId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdPackageId",
                principalSchema: "dbo",
                principalTable: "AdPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Payments_PaymentId",
                schema: "dbo",
                table: "Advertisements",
                column: "PaymentId",
                principalSchema: "dbo",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_AdPackages_AdPackageId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Payments_PaymentId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_AdPackageId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_ClinicId_Status",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_PaymentId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AdPackageId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
