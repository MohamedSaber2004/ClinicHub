using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                schema: "dbo",
                table: "Notifications",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "Body",
                schema: "dbo",
                table: "Notifications",
                newName: "BodyEn");

            migrationBuilder.AddColumn<string>(
                name: "BodyAr",
                schema: "dbo",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                schema: "dbo",
                table: "Notifications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyAr",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                schema: "dbo",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "BodyEn",
                schema: "dbo",
                table: "Notifications",
                newName: "Body");
        }
    }
}
