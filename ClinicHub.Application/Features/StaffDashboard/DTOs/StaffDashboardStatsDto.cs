namespace ClinicHub.Application.Features.StaffDashboard.DTOs
{
    public class StaffDashboardStatsDto
    {
        public int TotalAppointments { get; set; }
        public int CheckedIn { get; set; }
        public int Waiting { get; set; }
        public int Completed { get; set; }
    }
}
