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
}
