using ClinicHub.Application.Features.Appointments.DTOs;

namespace ClinicHub.Application.Features.DoctorDashboard.DTOs
{
    public class DoctorDashboardStatsDto
    {
        /// <summary>Total appointments scheduled for today (all statuses).</summary>
        public int TodayAppointmentsCount { get; set; }

        /// <summary>All-time distinct patients who completed at least one appointment with this doctor.</summary>
        public int TotalPatientsCount { get; set; }

        /// <summary>Appointments currently in Pending status (today).</summary>
        public int PendingAppointmentsCount { get; set; }

        /// <summary>All-time completed appointments count.</summary>
        public int CompletedAppointmentsCount { get; set; }

        // Extended stats for dashboard cards
        public int AcceptedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int TotalPatientsThisWeek { get; set; }
        public AppointmentDto? NextAppointment { get; set; }
    }
}
