using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addclinicsettingsfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerName",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleDoctor",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerName",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "ResponsibleDoctor",
                schema: "dbo",
                table: "Clinics");
        }
    }
}
