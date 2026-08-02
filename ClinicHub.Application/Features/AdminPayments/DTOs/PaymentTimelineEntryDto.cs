namespace ClinicHub.Application.Features.AdminPayments.DTOs;

public class PaymentTimelineEntryDto
{
    public DateTime Date { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Marker { get; set; } = "info";
}
