using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

internal static class AvailabilityValidation
{
    public static readonly TimeSpan ValidationHorizon = TimeSpan.FromDays(14);
    public static readonly TimeSpan DedupeWindow = TimeSpan.FromHours(24);

    public static readonly AppointmentStatus[] ActiveStatuses =
    {
        AppointmentStatus.Pending,
        AppointmentStatus.Reserved,
        AppointmentStatus.Accepted,
        AppointmentStatus.Confirmed
    };

    public static bool IsWithinClinicWorkingHours(Clinic? clinic, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
    {
        if (clinic?.WorkingHoursStart is null || clinic.WorkingHoursEnd is null)
            return true;

        var dayOfWeek = appointmentDate.DayOfWeek;
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

    public static async Task<HashSet<(Guid UserId, NotificationType Type)>> GetRecentlyNotifiedKeysAsync(
        IUnitOfWork unitOfWork, IEnumerable<NotificationType> types, DateTime now, CancellationToken cancellationToken)
    {
        var recent = await unitOfWork.GetRepository<Notification, Guid>()
            .GetAllAsync(n => types.Contains(n.Type) && n.CreatedAt > now.Add(-DedupeWindow))
            .Select(n => new { n.UserId, n.Type })
            .ToListAsync(cancellationToken);

        return recent.Select(n => (n.UserId, n.Type)).ToHashSet();
    }

    public static async Task<Dictionary<Guid, Guid>> ResolveClinicAdminIdsAsync(
        IUnitOfWork unitOfWork, IEnumerable<Clinic?> clinics, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, Guid>();

        var missing = clinics
            .Where(c => c != null && c.ClinicAdminId == null)
            .Select(c => c!.Id)
            .Distinct()
            .ToList();

        if (missing.Count > 0)
        {
            var adminUsers = await unitOfWork.GetRepository<ApplicationUser, Guid>()
                .GetAllAsync(u => u.ClinicId != null && missing.Contains(u.ClinicId.Value) && !u.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var user in adminUsers.Where(u => u.ClinicId.HasValue))
            {
                if (!result.ContainsKey(user.ClinicId!.Value))
                    result[user.ClinicId!.Value] = user.Id;
            }
        }

        foreach (var clinic in clinics.Where(c => c != null && c.ClinicAdminId.HasValue))
        {
            if (!result.ContainsKey(clinic!.Id))
                result[clinic!.Id] = clinic.ClinicAdminId!.Value;
        }

        return result;
    }
}
