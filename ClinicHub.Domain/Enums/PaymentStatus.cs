namespace ClinicHub.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
    Processing = 4,
    PartiallyRefunded = 5,
    RefundPending = 6,
    RefundFailed = 7
}