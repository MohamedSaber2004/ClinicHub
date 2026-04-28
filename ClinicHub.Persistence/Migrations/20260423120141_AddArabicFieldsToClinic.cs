using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArabicFieldsToClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressAr",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                schema: "dbo",
                table: "Clinics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("CREATE SPATIAL INDEX [IX_Clinics_Location] ON [dbo].[Clinics] ([Location])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clinics_Location",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "AddressAr",
                schema: "dbo",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "NameAr",
                schema: "dbo",
                table: "Clinics");
        }
    }
}
