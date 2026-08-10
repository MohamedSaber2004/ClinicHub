using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatenotificationtbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SenderUserId",
                schema: "dbo",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderUserId",
                schema: "dbo",
                table: "Notifications",
                column: "SenderUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_SenderUserId",
                schema: "dbo",
                table: "Notifications",
                column: "SenderUserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_SenderUserId",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SenderUserId",
                schema: "dbo",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                schema: "dbo",
                table: "Notifications");
        }
    }
}
