using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutBatchesAndNationalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalIdEncrypted",
                table: "ClinicPayoutAccounts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NationalIdLast4",
                table: "ClinicPayoutAccounts",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            // Paymob refuses a disbursement without a national id, so every account saved
            // before this column existed has to be re-verified by the clinic first.
            migrationBuilder.Sql(
                "UPDATE ClinicPayoutAccounts SET Status = 0, VerifiedAt = NULL WHERE NationalIdEncrypted = N'';");

            migrationBuilder.CreateTable(
                name: "PayoutBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PeriodFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemsCount = table.Column<int>(type: "int", nullable: false),
                    BudgetBefore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BudgetAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_PayoutBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayoutWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymobTransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Issuer = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StatusCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisbursementStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RawPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PayoutWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayoutItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayoutAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymobTransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymobStatusCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisbursementStatusRaw = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_PayoutItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutItems_ClinicPayoutAccounts_PayoutAccountId",
                        column: x => x.PayoutAccountId,
                        principalTable: "ClinicPayoutAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayoutItems_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayoutItems_PayoutBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "PayoutBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicLedgerEntries_PayoutItemId",
                table: "ClinicLedgerEntries",
                column: "PayoutItemId",
                unique: true,
                filter: "[PayoutItemId] IS NOT NULL AND [Type] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBatches_Code",
                table: "PayoutBatches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBatches_Status",
                table: "PayoutBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutItems_BatchId",
                table: "PayoutItems",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutItems_ClinicId_Status",
                table: "PayoutItems",
                columns: new[] { "ClinicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutItems_PaymobTransactionId",
                table: "PayoutItems",
                column: "PaymobTransactionId",
                unique: true,
                filter: "[PaymobTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutItems_PayoutAccountId",
                table: "PayoutItems",
                column: "PayoutAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutWebhookEvents_PaymobTransactionId",
                table: "PayoutWebhookEvents",
                column: "PaymobTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutWebhookEvents_ProcessedAt",
                table: "PayoutWebhookEvents",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutItems");

            migrationBuilder.DropTable(
                name: "PayoutWebhookEvents");

            migrationBuilder.DropTable(
                name: "PayoutBatches");

            migrationBuilder.DropIndex(
                name: "IX_ClinicLedgerEntries_PayoutItemId",
                table: "ClinicLedgerEntries");

            migrationBuilder.DropColumn(
                name: "NationalIdEncrypted",
                table: "ClinicPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "NationalIdLast4",
                table: "ClinicPayoutAccounts");
        }
    }
}
