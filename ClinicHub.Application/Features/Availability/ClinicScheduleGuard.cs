using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Availability
{
    /// <summary>
    /// Single source of truth for "a time window fits inside the clinic schedule".
    /// Uses the exact same containment semantics as booking validation
    /// (<c>ClinicIsOpen</c>): the day must be in <c>WorkingDays</c> (when configured)
    /// and [start, end] must sit inside [WorkingHoursStart, WorkingHoursEnd]
    /// (when configured). A clinic without configured hours accepts any window,
    /// so enforcing this guard at availability setup time guarantees that any
    /// configured doctor slot stays bookable later.
    /// </summary>
    public static class ClinicScheduleGuard
    {
        public static bool IsWithinClinicSchedule(Clinic? clinic, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            if (clinic?.WorkingHoursStart is null || clinic.WorkingHoursEnd is null)
                return true;

            var workingDays = ParseWorkingDays(clinic.WorkingDays);
            if (workingDays.Count > 0 && !workingDays.Contains(dayOfWeek))
                return false;

            return TimeOnly.FromTimeSpan(startTime) >= clinic.WorkingHoursStart.Value
                && TimeOnly.FromTimeSpan(endTime) <= clinic.WorkingHoursEnd.Value;
        }

        public static HashSet<DayOfWeek> ParseWorkingDays(string? workingDays)
        {
            var result = new HashSet<DayOfWeek>();
            if (string.IsNullOrWhiteSpace(workingDays))
                return result;

            foreach (var part in workingDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Enum.TryParse<DayOfWeek>(part, true, out var day))
                    result.Add(day);
            }

            return result;
        }
    }
}
