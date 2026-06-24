using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameMaxFutureDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_BookingReference",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "BookingReference",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "MaxFutureDays",
                schema: "dbo",
                table: "BookingConfigurations",
                newName: "MaxAdvanceBookingDays");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "ArDescription",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClinicAdminId",
                schema: "dbo",
                table: "Clinics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDays",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingHours",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WorkingHoursEnd",
                schema: "dbo",
                table: "Clinics",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WorkingHoursStart",
                schema: "dbo",
                table: "Clinics",
                type: "time",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_ClinicAdminId",
                schema: "dbo",
                table: "Clinics",
                column: "ClinicAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clinics_Users_ClinicAdminId",
                schema: "dbo",
                table: "Clinics",
                column: "ClinicAdminId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clinics_Users_ClinicAdminId",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_Clinics_ClinicAdminId",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "ArDescription",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "ClinicAdminId",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Logo",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "WorkingHours",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "WorkingHoursEnd",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "WorkingHoursStart",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.RenameColumn(
                name: "MaxAdvanceBookingDays",
                schema: "dbo",
                table: "BookingConfigurations",
                newName: "MaxFutureDays");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookingReference",
                schema: "dbo",
                table: "Appointments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BookingReference",
                schema: "dbo",
                table: "Appointments",
                column: "BookingReference",
                unique: true,
                filter: "[BookingReference] IS NOT NULL");
        }
    }
}
