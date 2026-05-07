using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "UserRefreshTokens",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "UserFbTokens",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "Specializations",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "Reactions",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "Posts",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "Notifications",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "Media",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "Comments",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "dbo",
                table: "Clinics",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "UserFbTokens");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "Specializations");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "dbo",
                table: "Clinics");
        }
    }
}
