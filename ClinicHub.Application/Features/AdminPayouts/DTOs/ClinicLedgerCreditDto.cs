namespace ClinicHub.Application.Features.AdminPayouts.DTOs;

public class ClinicLedgerCreditDto
{
    public Guid EntryId { get; set; }
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public decimal AvailableBalance { get; set; }
}
