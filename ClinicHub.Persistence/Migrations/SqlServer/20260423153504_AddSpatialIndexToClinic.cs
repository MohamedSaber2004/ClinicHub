using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpatialIndexToClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX IX_Clinics_Location_Spatial
                ON dbo.Clinics(Location)
                USING GEOGRAPHY_AUTO_GRID;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_Clinics_Location_Spatial ON dbo.Clinics;");
        }
    }
}
