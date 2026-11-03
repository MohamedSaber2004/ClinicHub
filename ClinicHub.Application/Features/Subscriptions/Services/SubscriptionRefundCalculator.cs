namespace ClinicHub.Application.Features.Subscriptions.Services;

public sealed record RefundQuote(int TotalDays, int UsedDays, decimal ChargeAmount, decimal RefundAmount);

/// <summary>
/// Single source of truth for the pro-rated cancellation math. Shared by the quote
/// endpoint, owner cancellation, admin cancellation, and the refund retry job.
/// </summary>
public static class SubscriptionRefundCalculator
{
    /// <summary>
    /// Computes how many days were used and how much of <paramref name="amountPaid"/>
    /// should be charged vs refunded. <paramref name="alreadyRefunded"/> is subtracted
    /// from the basis so the refund never exceeds the remaining paid amount.
    /// </summary>
    public static RefundQuote Calculate(
        decimal amountPaid,
        decimal alreadyRefunded,
        DateTime startDate,
        DateTime endDate,
        DateTime cancelledAt)
    {
        // Real calendar days so a 31-day month charges 2/31 and a year charges 2/365 (or /366).
        var totalDays = Math.Max(1, (endDate.Date - startDate.Date).Days);

        // Same-day cancellation still costs one day; cannot exceed the period length.
        var usedDays = Math.Clamp((cancelledAt.Date - startDate.Date).Days, 1, totalDays);

        var basis = Math.Max(0m, amountPaid - alreadyRefunded);
        var charge = Math.Round(basis * usedDays / totalDays, 2, MidpointRounding.AwayFromZero);
        var refund = Math.Max(0m, Math.Round(basis - charge, 2, MidpointRounding.AwayFromZero));

        return new RefundQuote(totalDays, usedDays, charge, refund);
    }
}
