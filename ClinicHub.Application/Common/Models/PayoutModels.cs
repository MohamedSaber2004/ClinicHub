namespace ClinicHub.Application.Common.Models;

/// <summary>
/// How a disbursement call ended from our side. Anything other than
/// <see cref="Accepted"/> must never be treated as "Paymob has nothing": a timeout leaves the
/// money possibly sent, so the item is inquired by reference before it is retried.
/// </summary>
public enum PayoutOutcome
{
    Accepted = 0,
    Rejected = 1,
    Unknown = 2
}

public class PayoutBudgetResult
{
    public bool Success { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? StatusCode { get; set; }
    public string? StatusDescription { get; set; }
}

public class PayoutBankDisbursementRequest
{
    public required string FullName { get; set; }
    public decimal Amount { get; set; }
    public required string BankCardNumber { get; set; }
    public required string BankCode { get; set; }
    public required string BankTransactionType { get; set; }
    public required string NationalId { get; set; }

    /// <summary>Stable uuid4 per payout item (our own id) — the idempotency anchor.</summary>
    public required Guid ClientReferenceId { get; set; }
}

public class PayoutDisbursementResult
{
    public PayoutOutcome Outcome { get; set; }
    public string? TransactionId { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusDescription { get; set; }
    public string? DisbursementStatus { get; set; }

    public bool Accepted => Outcome == PayoutOutcome.Accepted && !string.IsNullOrWhiteSpace(TransactionId);
}

public class PayoutTransactionStatus
{
    public string TransactionId { get; set; } = string.Empty;
    public string? ClientReferenceId { get; set; }
    public string? DisbursementStatus { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusDescription { get; set; }
    public decimal? Amount { get; set; }

    public bool IsSuccessful => string.Equals(DisbursementStatus, "successful", StringComparison.OrdinalIgnoreCase);
    public bool IsFailed => string.Equals(DisbursementStatus, "failed", StringComparison.OrdinalIgnoreCase);
    public bool IsPending => !IsSuccessful && !IsFailed;
}
