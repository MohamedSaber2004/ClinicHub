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

    /// <summary>Superadmin platform-fee cut backed out of <see cref="TodayRevenue"/>.</summary>
    public decimal TodayPlatformFees { get; set; }

    /// <summary>Clinic net for today: <see cref="TodayRevenue"/> minus <see cref="TodayPlatformFees"/>.</summary>
    public decimal TodayNetRevenue { get; set; }

    /// <summary>Superadmin platform-fee cut backed out of <see cref="MonthRevenue"/>.</summary>
    public decimal MonthPlatformFees { get; set; }

    /// <summary>Clinic net for the month: <see cref="MonthRevenue"/> minus <see cref="MonthPlatformFees"/>.</summary>
    public decimal MonthNetRevenue { get; set; }

    /// <summary>Superadmin platform-fee cut backed out of <see cref="PaidTotal"/>.</summary>
    public decimal PaidPlatformFees { get; set; }

    /// <summary>Clinic net all-time collected: <see cref="PaidTotal"/> minus <see cref="PaidPlatformFees"/>.</summary>
    public decimal PaidNetTotal { get; set; }

    /// <summary>Superadmin platform-fee cut backed out of <see cref="PendingTotal"/>.</summary>
    public decimal PendingPlatformFees { get; set; }

    /// <summary>Clinic net awaiting collection: <see cref="PendingTotal"/> minus <see cref="PendingPlatformFees"/>.</summary>
    public decimal PendingNetTotal { get; set; }
}
