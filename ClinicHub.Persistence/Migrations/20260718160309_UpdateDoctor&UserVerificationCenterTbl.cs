using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorUserVerificationCenterTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Clinics_ClinicId",
                schema: "dbo",
                table: "Doctors");

            migrationBuilder.DropTable(
                name: "ClinicVerifications",
                schema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                schema: "dbo",
                table: "UserVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecializationId",
                schema: "dbo",
                table: "UserVerifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                schema: "dbo",
                table: "UserVerifications",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "Doctors",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Clinics_ClinicId",
                schema: "dbo",
                table: "Doctors",
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
                name: "FK_Doctors_Clinics_ClinicId",
                schema: "dbo",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Bio",
                schema: "dbo",
                table: "UserVerifications");

            migrationBuilder.DropColumn(
                name: "SpecializationId",
                schema: "dbo",
                table: "UserVerifications");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                schema: "dbo",
                table: "UserVerifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClinicId",
                schema: "dbo",
                table: "Doctors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicVerifications",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicVerifications_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "dbo",
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicVerifications_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicVerifications_ClinicId",
                schema: "dbo",
                table: "ClinicVerifications",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicVerifications_ReviewedByUserId",
                schema: "dbo",
                table: "ClinicVerifications",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Clinics_ClinicId",
                schema: "dbo",
                table: "Doctors",
                column: "ClinicId",
                principalSchema: "dbo",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
