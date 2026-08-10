using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class implementtenancybasedtechnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "SupportTickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "DoctorAvailabilities",
                type: "uniqueidentifier",
                nullable: true);

            // Backfill existing DoctorAvailabilities rows from their Doctor's clinic
            migrationBuilder.Sql(@"
                UPDATE da
                SET da.ClinicId = d.ClinicId
                FROM [dbo].[DoctorAvailabilities] da
                INNER JOIN [dbo].[Doctors] d ON d.Id = da.DoctorId");

            // Backfill existing Payments rows from their Appointment's clinic
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.ClinicId = a.ClinicId
                FROM [dbo].[Payments] p
                INNER JOIN [dbo].[Appointments] a ON a.Id = p.AppointmentId");

            // Now safe to make non-nullable since all rows have valid ClinicId values
            migrationBuilder.AlterColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "DoctorAvailabilities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "UserClinics",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_UserClinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClinics_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "dbo",
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserClinics_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_ClinicId",
                schema: "dbo",
                table: "SupportTickets",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClinicId",
                schema: "dbo",
                table: "Payments",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ClinicId",
                schema: "dbo",
                table: "Notifications",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_ClinicId",
                schema: "dbo",
                table: "DoctorAvailabilities",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClinics_ClinicId",
                schema: "dbo",
                table: "UserClinics",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClinics_UserId_ClinicId",
                schema: "dbo",
                table: "UserClinics",
                columns: new[] { "UserId", "ClinicId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailabilities_Clinics_ClinicId",
                schema: "dbo",
                table: "DoctorAvailabilities",
                column: "ClinicId",
                principalSchema: "dbo",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Clinics_ClinicId",
                schema: "dbo",
                table: "Notifications",
                column: "ClinicId",
                principalSchema: "dbo",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Clinics_ClinicId",
                schema: "dbo",
                table: "Payments",
                column: "ClinicId",
                principalSchema: "dbo",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Clinics_ClinicId",
                schema: "dbo",
                table: "SupportTickets",
                column: "ClinicId",
                principalSchema: "dbo",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailabilities_Clinics_ClinicId",
                schema: "dbo",
                table: "DoctorAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Clinics_ClinicId",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Clinics_ClinicId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Clinics_ClinicId",
                schema: "dbo",
                table: "SupportTickets");

            migrationBuilder.DropTable(
                name: "UserClinics",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_ClinicId",
                schema: "dbo",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ClinicId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ClinicId",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailabilities_ClinicId",
                schema: "dbo",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                schema: "dbo",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                schema: "dbo",
                table: "DoctorAvailabilities");
        }
    }
}
