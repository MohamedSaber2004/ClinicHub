namespace ClinicHub.Application.Features.AdminPayments.DTOs;

/// <summary>
/// Monthly (or ranged) money summary for one clinic: patient-paid gross,
/// superadmin platform-fee cut (appointment payments only) and clinic net.
/// Net + fees always equal the appointment gross.
/// </summary>
public class ClinicPaymentsSummaryDto
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    /// <summary>Paid payments count in range (all types).</summary>
    public int PaymentsCount { get; set; }
    /// <summary>Patient-paid gross in range (all payment types).</summary>
    public decimal TotalRevenue { get; set; }
    /// <summary>Superadmin cut backed out of the appointment gross.</summary>
    public decimal PlatformFees { get; set; }
    /// <summary>Clinic net: appointment gross minus <see cref="PlatformFees"/>.</summary>
    public decimal NetRevenue { get; set; }
}
