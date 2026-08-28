using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdsBusinessTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAds",
                schema: "public",
                table: "AdPackages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxImpressions",
                schema: "public",
                table: "AdPackages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAds",
                schema: "public",
                table: "AdPackages");

            migrationBuilder.DropColumn(
                name: "MaxImpressions",
                schema: "public",
                table: "AdPackages");
        }
    }
}
