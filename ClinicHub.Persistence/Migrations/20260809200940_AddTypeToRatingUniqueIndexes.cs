using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToRatingUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_ClinicId",
                schema: "dbo",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_DoctorId",
                schema: "dbo",
                table: "Ratings");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_ClinicId_Type",
                schema: "dbo",
                table: "Ratings",
                columns: new[] { "UserId", "ClinicId", "Type" },
                unique: true,
                filter: "[ClinicId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_DoctorId_Type",
                schema: "dbo",
                table: "Ratings",
                columns: new[] { "UserId", "DoctorId", "Type" },
                unique: true,
                filter: "[DoctorId] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_ClinicId_Type",
                schema: "dbo",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_DoctorId_Type",
                schema: "dbo",
                table: "Ratings");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_ClinicId",
                schema: "dbo",
                table: "Ratings",
                columns: new[] { "UserId", "ClinicId" },
                unique: true,
                filter: "[ClinicId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_DoctorId",
                schema: "dbo",
                table: "Ratings",
                columns: new[] { "UserId", "DoctorId" },
                unique: true,
                filter: "[DoctorId] IS NOT NULL");
        }
    }
}
