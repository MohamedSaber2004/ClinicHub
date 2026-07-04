using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateclinicstbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId1",
                schema: "dbo",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ClinicId1",
                schema: "dbo",
                table: "Subscriptions",
                column: "ClinicId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Clinics_ClinicId1",
                schema: "dbo",
                table: "Subscriptions",
                column: "ClinicId1",
                principalSchema: "dbo",
                principalTable: "Clinics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Clinics_ClinicId1",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_ClinicId1",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ClinicId1",
                schema: "dbo",
                table: "Subscriptions");
        }
    }
}
