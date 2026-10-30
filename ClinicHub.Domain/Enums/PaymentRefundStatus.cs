namespace ClinicHub.Domain.Enums;

public enum PaymentRefundStatus
{
    None = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    NeedsReview = 5
}
