using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyBookingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowOnlineBooking",
                schema: "dbo",
                table: "BookingConfigurations");

            migrationBuilder.DropColumn(
                name: "PaymentMethods",
                schema: "dbo",
                table: "BookingConfigurations");

            migrationBuilder.DropColumn(
                name: "RequirePayment",
                schema: "dbo",
                table: "BookingConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowOnlineBooking",
                schema: "dbo",
                table: "BookingConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethods",
                schema: "dbo",
                table: "BookingConfigurations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "credit_card,cash");

            migrationBuilder.AddColumn<bool>(
                name: "RequirePayment",
                schema: "dbo",
                table: "BookingConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
