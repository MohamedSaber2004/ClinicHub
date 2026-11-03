using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayouts.DTOs;

public class PayoutBatchDto
{
    public static PayoutBatchDto From(PayoutBatch batch, int maxAttempts)
    {
        var items = batch.Items.Select(item => PayoutItemDto.From(item, maxAttempts)).ToList();

        return new PayoutBatchDto
        {
            Id = batch.Id,
            Code = batch.Code,
            Status = batch.Status,
            PeriodFrom = batch.PeriodFrom,
            PeriodTo = batch.PeriodTo,
            TotalAmount = batch.TotalAmount,
            ItemsCount = batch.ItemsCount,
            SuccessCount = items.Count(i => i.Status == PayoutItemStatus.Success),
            FailedCount = items.Count(i => i.Status is PayoutItemStatus.Failed or PayoutItemStatus.Cancelled),
            PendingCount = items.Count(i => i.Status is PayoutItemStatus.Pending or PayoutItemStatus.Processing),
            SuccessAmount = items.Where(i => i.Status == PayoutItemStatus.Success).Sum(i => i.Amount),
            FailedAmount = items.Where(i => i.Status is PayoutItemStatus.Failed or PayoutItemStatus.Cancelled).Sum(i => i.Amount),
            BudgetBefore = batch.BudgetBefore,
            BudgetAfter = batch.BudgetAfter,
            ApprovedAt = batch.ApprovedAt,
            ExecutedAt = batch.ExecutedAt,
            CompletedAt = batch.CompletedAt,
            FailureNotes = batch.FailureNotes,
            Currency = items.FirstOrDefault()?.Currency ?? "EGP",
            Items = items
        };
    }

    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public PayoutBatchStatus Status { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemsCount { get; set; }
    public decimal SuccessAmount { get; set; }
    public decimal FailedAmount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public decimal? BudgetBefore { get; set; }
    public decimal? BudgetAfter { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureNotes { get; set; }
    public string Currency { get; set; } = "EGP";
    public List<PayoutItemDto> Items { get; set; } = new();
}

public class PayoutItemDto
{
    public static PayoutItemDto From(PayoutItem item, int maxAttempts) => new()
    {
        Id = item.Id,
        BatchId = item.BatchId,
        BatchCode = item.Batch?.Code ?? string.Empty,
        ClinicId = item.ClinicId,
        ClinicName = item.Clinic?.Name ?? string.Empty,
        Amount = item.Amount,
        Currency = item.Currency,
        Status = item.Status,
        BankCode = item.PayoutAccount?.BankCode ?? string.Empty,
        MaskedAccountNumber = item.PayoutAccount is null
            ? string.Empty
            : $"**** {item.PayoutAccount.AccountLast4}",
        HolderName = item.PayoutAccount?.HolderName ?? string.Empty,
        PaymobTransactionId = item.PaymobTransactionId,
        PaymobStatusCode = item.PaymobStatusCode,
        DisbursementStatus = item.DisbursementStatusRaw,
        LastError = item.LastError,
        Attempts = item.Attempts,
        CanRetry = item.CanRetry(maxAttempts),
        SentAt = item.SentAt,
        SettledAt = item.SettledAt
    };

    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PayoutItemStatus Status { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string MaskedAccountNumber { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public string? PaymobTransactionId { get; set; }
    public string? PaymobStatusCode { get; set; }
    public string? DisbursementStatus { get; set; }
    public string? LastError { get; set; }
    public int Attempts { get; set; }
    public bool CanRetry { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? SettledAt { get; set; }
}
