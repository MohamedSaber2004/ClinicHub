using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicePlatformToUserFbTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                schema: "dbo",
                table: "UserFbTokens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "DevicePlatform",
                schema: "dbo",
                table: "UserFbTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserFbTokens_Token",
                schema: "dbo",
                table: "UserFbTokens",
                column: "Token",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserFbTokens_Token",
                schema: "dbo",
                table: "UserFbTokens");

            migrationBuilder.DropColumn(
                name: "DevicePlatform",
                schema: "dbo",
                table: "UserFbTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                schema: "dbo",
                table: "UserFbTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
