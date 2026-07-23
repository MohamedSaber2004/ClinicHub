namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class StaffDashboardStatsDto
    {
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int AcceptedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CheckedInCount { get; set; }
        public int QueueLength { get; set; }
    }
}
