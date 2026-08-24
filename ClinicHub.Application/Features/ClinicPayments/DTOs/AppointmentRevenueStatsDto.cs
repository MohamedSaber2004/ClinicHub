namespace ClinicHub.Application.Features.ClinicPayments.DTOs;

public class AppointmentRevenueStatsDto
{
    /// <summary>Sum of appointment payments actually received (Paid) today.</summary>
    public decimal TodayRevenue { get; set; }

    /// <summary>Sum of appointment payments received during the current month.</summary>
    public decimal MonthRevenue { get; set; }

    /// <summary>All-time collected appointment payments.</summary>
    public decimal PaidTotal { get; set; }

    /// <summary>Pending/processing appointment payments awaiting collection.</summary>
    public decimal PendingTotal { get; set; }
}
