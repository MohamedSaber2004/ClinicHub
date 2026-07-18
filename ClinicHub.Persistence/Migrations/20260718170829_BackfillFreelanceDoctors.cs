using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillFreelanceDoctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [dbo].[Doctors] (
                    [Id], [UserId], [ClinicId], [SpecializationId], [Bio], [YearsOfExperience],
                    [IsFreelance], [IsActive], [IsDeleted],
                    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
                    [DeletedAt], [DeletedBy]
                )
                SELECT
                    NEWID(),
                    u.[Id],
                    NULL,
                    ISNULL(uv.[SpecializationId], (SELECT TOP 1 [Id] FROM [dbo].[Specializations])),
                    ISNULL(uv.[Bio], N''),
                    ISNULL(uv.[YearsOfExperience], 0),
                    1, 1, 0,
                    GETDATE(), N'BackfillMigration', NULL, NULL,
                    NULL, NULL
                FROM [dbo].[Users] u
                INNER JOIN [dbo].[AspNetUserRoles] ur ON u.[Id] = ur.[UserId]
                INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] AND r.[Name] = N'Doctor'
                INNER JOIN [dbo].[UserVerifications] uv ON uv.[UserId] = u.[Id]
                    AND uv.[RequestedRole] = 4 AND uv.[Status] = 1 AND uv.[IsDeleted] = 0
                LEFT JOIN [dbo].[Doctors] d ON d.[UserId] = u.[Id]
                WHERE d.[Id] IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [dbo].[Doctors]
                WHERE [CreatedBy] = N'BackfillMigration'
                    AND [IsFreelance] = 1 AND [ClinicId] IS NULL
                """);
        }
    }
}
