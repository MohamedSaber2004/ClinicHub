namespace ClinicHub.Application.Common
{
    public sealed record AppointmentRevenueSplit(decimal Total, decimal PlatformFee, decimal ClinicNet);

    public static class AppointmentRevenueSplitter
    {
        public static AppointmentRevenueSplit Split(decimal grossTotal, decimal percent)
        {
            if (percent <= 0)
                return new AppointmentRevenueSplit(grossTotal, 0m, grossTotal);
            var net = Math.Round(grossTotal / (1m + percent / 100m), 2, MidpointRounding.AwayFromZero);
            return new AppointmentRevenueSplit(grossTotal, grossTotal - net, net);
        }

        public static (decimal Fees, decimal Net) SumSplits(IEnumerable<decimal> grossTotals, decimal percent)
        {
            decimal fees = 0m, net = 0m;
            foreach (var gross in grossTotals)
            {
                var split = Split(gross, percent);
                fees += split.PlatformFee;
                net += split.ClinicNet;
            }
            return (fees, net);
        }
    }
}
