namespace ClinicHub.Application.Common
{
    /// <summary>
    /// Timezone-free date helper. The platform stores and compares plain wall-clock
    /// calendar dates — no offsets, no UTC/Cairo conversions, no <see cref="DateTimeKind"/>
    /// shifts. Every incoming <see cref="DateTime"/> is reduced to its calendar date
    /// as <see cref="DateTimeKind.Unspecified"/> midnight before any DayOfWeek,
    /// past/future, or booking-window check.
    /// </summary>
    public static class AppDate
    {
        public static DateTime ToUnzonedDate(DateTime value)
            => new(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Unspecified);

        public static DateTime? ToUnzonedDate(DateTime? value)
            => value.HasValue ? ToUnzonedDate(value.Value) : null;

        /// <summary>Server wall-clock, no conversion.</summary>
        public static DateTime Now => DateTime.Now;

        /// <summary>Server calendar date as Unspecified midnight.</summary>
        public static DateTime Today => ToUnzonedDate(DateTime.Now);
    }
}
