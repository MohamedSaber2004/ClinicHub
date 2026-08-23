namespace ClinicHub.Application.Features.Admin.Queries.Common
{
    public enum GraphGranularity
    {
        Day = 0,
        Week = 1,
        Month = 2
    }

    public static class GraphPeriodHelper
    {
        private const int MaxRangeDays = 400;

        public static GraphGranularity ParseGranularity(string? granularity)
        {
            return granularity?.Trim().ToLowerInvariant() switch
            {
                "week" => GraphGranularity.Week,
                "month" => GraphGranularity.Month,
                _ => GraphGranularity.Day
            };
        }

        public static (DateTime FromDate, DateTime ToDate) NormalizeRange(DateTime? fromDate, DateTime? toDate)
        {
            var to = toDate?.Date ?? DateTime.Today;
            var from = fromDate?.Date ?? to.AddDays(-30);

            if (from > to)
                (from, to) = (to, from);

            if ((to - from).TotalDays > MaxRangeDays)
                from = to.AddDays(-MaxRangeDays);

            return (from, to.AddDays(1));
        }

        public static List<DateTime> BuildBuckets(DateTime fromDateInclusive, DateTime toDateExclusive, GraphGranularity granularity)
        {
            var buckets = new List<DateTime>();
            var current = BucketStart(fromDateInclusive, granularity);

            while (current < toDateExclusive)
            {
                buckets.Add(current);
                current = NextBucketStart(current, granularity);
            }

            return buckets;
        }

        public static DateTime BucketStart(DateTime date, GraphGranularity granularity)
        {
            return granularity switch
            {
                GraphGranularity.Week => date.Date.AddDays(-(((int)date.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7)),
                GraphGranularity.Month => new DateTime(date.Year, date.Month, 1),
                _ => date.Date
            };
        }

        public static string FormatBucket(DateTime bucket, GraphGranularity granularity)
        {
            return granularity == GraphGranularity.Month
                ? bucket.ToString("yyyy-MM")
                : bucket.ToString("yyyy-MM-dd");
        }

        private static DateTime NextBucketStart(DateTime bucket, GraphGranularity granularity)
        {
            return granularity switch
            {
                GraphGranularity.Week => bucket.AddDays(7),
                GraphGranularity.Month => bucket.AddMonths(1),
                _ => bucket.AddDays(1)
            };
        }
    }
}
