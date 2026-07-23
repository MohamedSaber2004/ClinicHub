using ClinicHub.Application.Features.Appointments.DTOs;

namespace ClinicHub.Application.Features.DoctorDashboard.DTOs
{
    public class DoctorDashboardStatsDto
    {
        public int PendingAppointments { get; set; }
        public int AcceptedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int TotalPatientsThisWeek { get; set; }
        public AppointmentDto? NextAppointment { get; set; }
    }
}
