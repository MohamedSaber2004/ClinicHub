namespace ClinicHub.Domain.Enums;

public enum PayoutBatchStatus
{
    Draft = 0,
    Approved = 1,
    Processing = 2,
    Completed = 3,
    PartiallyFailed = 4,
    Failed = 5,
    Cancelled = 6
}
