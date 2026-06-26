using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSlotDurationMinutesFromBookingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotDurationMinutes",
                schema: "dbo",
                table: "BookingConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotDurationMinutes",
                schema: "dbo",
                table: "BookingConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 30);
        }
    }
}
