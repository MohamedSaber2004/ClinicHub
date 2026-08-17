namespace ClinicHub.Application.Features.Clinics.DTOs
{
    public class ClinicAdvancedReportDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalVisits { get; set; }
        public double CompletionRate { get; set; }
        public double TotalRevenue { get; set; }
        public double AverageAppointmentValue { get; set; }
        public List<DoctorRevenueDto> RevenueByDoctor { get; set; } = new();
        public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
        public List<BusiestDayDto> BusiestDays { get; set; } = new();
    }

    public class DoctorRevenueDto
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public int AppointmentCount { get; set; }
        public double Revenue { get; set; }
    }

    public class BusiestDayDto
    {
        public DateTime Date { get; set; }
        public int AppointmentCount { get; set; }
    }
}