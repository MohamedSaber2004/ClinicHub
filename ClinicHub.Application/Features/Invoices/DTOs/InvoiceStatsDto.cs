namespace ClinicHub.Application.Features.Invoices.DTOs;

public class InvoiceStatsDto
{
    public decimal TodayRevenue { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public int DraftCount { get; set; }
    public int CancelledCount { get; set; }
    public decimal InsuranceRatio { get; set; }
}
