namespace ClinicHub.Application.Features.Payment.DTOs;

/// <summary>
/// Result of asking Paymob directly about an order's payment state
/// (used as a fallback when the webhook has not arrived yet).
/// </summary>
public class PaymobOrderStatusDto
{
    public bool Found { get; set; }
    public bool Paid { get; set; }
    public long AmountCents { get; set; }
    public long PaidAmountCents { get; set; }
}
