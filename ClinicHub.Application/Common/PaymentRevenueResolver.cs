using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Common;

/// <summary>
/// Reads the revenue split captured when a payment was collected, falling back to a live
/// calculation only for legacy rows that predate the snapshot columns. Payouts and
/// statements must use the snapshot so later fee changes never rewrite history.
/// </summary>
public static class PaymentRevenueResolver
{
    public static AppointmentRevenueSplit Resolve(
        decimal gross,
        decimal? platformFee,
        decimal? clinicNet,
        decimal currentPercent)
    {
        if (platformFee.HasValue && clinicNet.HasValue)
            return new AppointmentRevenueSplit(gross, platformFee.Value, clinicNet.Value);

        return AppointmentRevenueSplitter.Split(gross, currentPercent);
    }

    public static AppointmentRevenueSplit Resolve(Payment payment, decimal currentPercent) =>
        Resolve(payment.Amount, payment.PlatformFeeAmount, payment.ClinicNetAmount, currentPercent);

    public static (decimal Fees, decimal Net) SumResolved(
        IEnumerable<(decimal Gross, decimal? PlatformFee, decimal? ClinicNet)> rows,
        decimal currentPercent)
    {
        decimal fees = 0m, net = 0m;
        foreach (var row in rows)
        {
            var split = Resolve(row.Gross, row.PlatformFee, row.ClinicNet, currentPercent);
            fees += split.PlatformFee;
            net += split.ClinicNet;
        }

        return (fees, net);
    }

    /// <summary>
    /// Same as the unrefunded overload, but each payment contributes only the part the clinic
    /// keeps: fee and net shrink in the ratio the payment was collected at, so a refund never
    /// re-prices history at the current percent.
    /// </summary>
    public static (decimal Fees, decimal Net) SumResolved(
        IEnumerable<(decimal Gross, decimal Refunded, decimal? PlatformFee, decimal? ClinicNet)> rows,
        decimal currentPercent)
    {
        decimal fees = 0m, net = 0m;
        foreach (var row in rows)
        {
            var kept = row.Gross - row.Refunded;
            if (kept <= 0m)
                continue;

            var split = Resolve(row.Gross, row.PlatformFee, row.ClinicNet, currentPercent);
            if (kept == split.Total)
            {
                fees += split.PlatformFee;
                net += split.ClinicNet;
                continue;
            }

            var share = kept / split.Total;
            fees += Math.Round(split.PlatformFee * share, 2, MidpointRounding.AwayFromZero);
            net += Math.Round(split.ClinicNet * share, 2, MidpointRounding.AwayFromZero);
        }

        return (fees, net);
    }
}
