using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                schema: "dbo",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                schema: "dbo",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RedirectUrl",
                schema: "dbo",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                schema: "dbo",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookingReference",
                schema: "dbo",
                table: "Appointments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                schema: "dbo",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                schema: "dbo",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "EGP"),
                    SlotDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    MaxFutureDays = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    ReservationTtlMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    PaymentMethods = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "credit_card,cash"),
                    AllowOnlineBooking = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequirePayment = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_BookingConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingConfigurations_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "dbo",
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BookingReference",
                schema: "dbo",
                table: "Appointments",
                column: "BookingReference",
                unique: true,
                filter: "[BookingReference] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PaymentId",
                schema: "dbo",
                table: "Appointments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingConfigurations_ClinicId",
                schema: "dbo",
                table: "BookingConfigurations",
                column: "ClinicId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Payments_PaymentId",
                schema: "dbo",
                table: "Appointments",
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
                name: "FK_Appointments_Payments_PaymentId",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "BookingConfigurations",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BookingReference",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PaymentId",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RedirectUrl",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BookingReference",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                schema: "dbo",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                schema: "dbo",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
