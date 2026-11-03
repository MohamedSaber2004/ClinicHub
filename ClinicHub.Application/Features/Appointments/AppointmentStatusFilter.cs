using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Appointments;

/// <summary>
/// Maps the mobile "My appointments" tab keys to <see cref="AppointmentStatus"/>.
/// The patient app names the <see cref="AppointmentStatus.Accepted"/> state
/// "AwaitingPayment", so both spellings resolve to the same status.
/// </summary>
public static class AppointmentStatusFilter
{
    public static bool TryResolve(string? status, out AppointmentStatus? resolved)
    {
        resolved = null;

        if (string.IsNullOrWhiteSpace(status))
            return true;

        var key = Normalize(status);

        if (key == "all")
            return true;

        resolved = key switch
        {
            "pending" => AppointmentStatus.Pending,
            "awaitingpayment" or "pendingpayment" or "unpaid" => AppointmentStatus.Accepted,
            "confirmed" => AppointmentStatus.Confirmed,
            "completed" => AppointmentStatus.Completed,
            "cancelled" or "canceled" => AppointmentStatus.Cancelled,
            "rejected" => AppointmentStatus.Rejected,
            _ => null
        };

        if (resolved.HasValue)
            return true;

        if (Enum.TryParse<AppointmentStatus>(status.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            resolved = parsed;
            return true;
        }

        return false;
    }

    private static string Normalize(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
