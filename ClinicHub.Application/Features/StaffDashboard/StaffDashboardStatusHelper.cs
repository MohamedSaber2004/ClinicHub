using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.StaffDashboard
{
    public static class StaffDashboardStatusHelper
    {
        public static string GetStatusValue(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "pending",
                AppointmentStatus.Reserved => "confirmed",
                AppointmentStatus.Confirmed => "confirmed",
                AppointmentStatus.Accepted => "confirmed",
                AppointmentStatus.Cancelled => "cancelled",
                AppointmentStatus.Rejected => "cancelled",
                AppointmentStatus.Completed => "completed",
                AppointmentStatus.NoShow => "completed",
                _ => "pending"
            };
        }

        public static string GetStatusLabel(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "\u0642\u064A\u062F \u0627\u0644\u0627\u0646\u062A\u0638\u0627\u0631",
                AppointmentStatus.Reserved => "\u0645\u0624\u0643\u062F",
                AppointmentStatus.Confirmed => "\u0645\u0624\u0643\u062F",
                AppointmentStatus.Accepted => "\u0645\u0624\u0643\u062F",
                AppointmentStatus.Cancelled => "\u0645\u0644\u063A\u064A",
                AppointmentStatus.Rejected => "\u0645\u0644\u063A\u064A",
                AppointmentStatus.Completed => "\u0645\u0646\u062A\u0647\u064A",
                AppointmentStatus.NoShow => "\u0645\u0646\u062A\u0647\u064A",
                _ => "\u0642\u064A\u062F \u0627\u0644\u0627\u0646\u062A\u0638\u0627\u0631"
            };
        }

        public static string GetStatusClass(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "badge-warning",
                AppointmentStatus.Reserved => "badge-success",
                AppointmentStatus.Confirmed => "badge-success",
                AppointmentStatus.Accepted => "badge-success",
                AppointmentStatus.Cancelled => "badge-danger",
                AppointmentStatus.Rejected => "badge-danger",
                AppointmentStatus.Completed => "badge-info",
                AppointmentStatus.NoShow => "badge-info",
                _ => "badge-warning"
            };
        }

        public static string GetQueueStatusValue(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Reserved => "registered",
                AppointmentStatus.Accepted => "waiting",
                AppointmentStatus.Confirmed => "in-progress",
                AppointmentStatus.Completed => "completed",
                _ => "waiting"
            };
        }

        public static string GetQueueStatusLabel(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Reserved => "\u062A\u0645 \u0627\u0644\u062A\u0633\u062C\u064A\u0644",
                AppointmentStatus.Accepted => "\u0641\u064A \u0627\u0644\u0627\u0646\u062A\u0638\u0627\u0631",
                AppointmentStatus.Confirmed => "\u0642\u064A\u062F \u0627\u0644\u0643\u0634\u0641",
                AppointmentStatus.Completed => "\u0645\u0643\u062A\u0645\u0644",
                _ => "\u0641\u064A \u0627\u0644\u0627\u0646\u062A\u0638\u0627\u0631"
            };
        }

        public static string GetQueueStatusClass(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Reserved => "badge-info",
                AppointmentStatus.Accepted => "badge-warning",
                AppointmentStatus.Confirmed => "badge-primary",
                AppointmentStatus.Completed => "badge-success",
                _ => "badge-warning"
            };
        }

        public static string GetInitial(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            return name.Trim()[..1];
        }
    }
}
