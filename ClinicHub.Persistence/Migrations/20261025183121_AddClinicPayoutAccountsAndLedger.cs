using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicPayoutAccountsAndLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ClinicNetAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFeeAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayoutItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicLedgerEntries_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicPayoutAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HolderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountNumberEncrypted = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AccountLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    BankTransactionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicPayoutAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicPayoutAccounts_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicLedgerEntries_ClinicId",
                table: "ClinicLedgerEntries",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicLedgerEntries_ClinicId_Status_AvailableAt",
                table: "ClinicLedgerEntries",
                columns: new[] { "ClinicId", "Status", "AvailableAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicLedgerEntries_PaymentId",
                table: "ClinicLedgerEntries",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL AND [Type] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPayoutAccounts_ClinicId",
                table: "ClinicPayoutAccounts",
                column: "ClinicId",
                unique: true);

            // Backfill revenue snapshots for already-collected appointment payments using the
            // fee percent in force at migration time. Formula mirrors AppointmentRevenueSplitter:
            // net = round(gross / (1 + percent / 100), 2), fee = gross - net.
            migrationBuilder.Sql("""
                DECLARE @feePercent decimal(18,4) = ISNULL(
                    (SELECT TOP 1 [AppointmentFeePercent] FROM [PlatformSettings] WHERE [IsDeleted] = 0),
                    0);

                UPDATE [Payments]
                SET [ClinicNetAmount] = CASE
                        WHEN @feePercent <= 0 THEN [Amount]
                        ELSE ROUND([Amount] / (1 + @feePercent / 100.0), 2)
                    END
                WHERE [Type] = 0 AND [Status] = 1 AND [IsDeleted] = 0 AND [ClinicNetAmount] IS NULL;

                UPDATE [Payments]
                SET [PlatformFeeAmount] = [Amount] - [ClinicNetAmount]
                WHERE [Type] = 0 AND [Status] = 1 AND [IsDeleted] = 0
                      AND [ClinicNetAmount] IS NOT NULL AND [PlatformFeeAmount] IS NULL;

                -- Seed the ledger with one earning per collected appointment payment. The refund
                -- hold uses the default 7 days; runtime accruals use PayoutSettings:RefundHoldDays.
                INSERT INTO [ClinicLedgerEntries]
                    ([Id], [ClinicId], [Type], [Amount], [Currency], [Status], [AvailableAt],
                     [PaymentId], [PayoutItemId], [Notes], [CreatedAt], [CreatedBy], [IsDeleted], [IsActive])
                SELECT
                    NEWID(),
                    p.[ClinicId],
                    0,
                    p.[ClinicNetAmount],
                    ISNULL(NULLIF(p.[Currency], ''), 'EGP'),
                    0,
                    DATEADD(DAY, 7, ISNULL(p.[PaidAt], p.[CreatedAt])),
                    p.[Id],
                    NULL,
                    N'Historical backfill',
                    ISNULL(p.[PaidAt], p.[CreatedAt]),
                    N'System',
                    0,
                    1
                FROM [Payments] p
                WHERE p.[Type] = 0
                      AND p.[Status] = 1
                      AND p.[IsDeleted] = 0
                      AND p.[AppointmentId] IS NOT NULL
                      AND p.[ClinicNetAmount] > 0
                      AND NOT EXISTS (
                          SELECT 1 FROM [ClinicLedgerEntries] e
                          WHERE e.[PaymentId] = p.[Id] AND e.[Type] = 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicLedgerEntries");

            migrationBuilder.DropTable(
                name: "ClinicPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "ClinicNetAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformFeeAmount",
                table: "Payments");
        }
    }
}
