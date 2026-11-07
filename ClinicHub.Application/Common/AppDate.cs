using ClinicHub.Domain.Common;

namespace ClinicHub.Application.Common
{
    /// <summary>
    /// Egypt wall-clock date helper. The platform stores and compares plain local
    /// calendar dates — no offsets, no UTC shifts. Every incoming <see cref="DateTime"/>
    /// is reduced to its calendar date as <see cref="DateTimeKind.Unspecified"/> midnight
    /// before any DayOfWeek, past/future, or booking-window check.
    /// </summary>
    public static class AppDate
    {
        public static DateTime ToUnzonedDate(DateTime value)
            => new(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Unspecified);

        public static DateTime? ToUnzonedDate(DateTime? value)
            => value.HasValue ? ToUnzonedDate(value.Value) : null;

        /// <summary>Egypt wall-clock, no conversion.</summary>
        public static DateTime Now => BaseEntity.AppNow;

        /// <summary>Egypt calendar date as Unspecified midnight.</summary>
        public static DateTime Today => ToUnzonedDate(BaseEntity.AppNow);
    }
}