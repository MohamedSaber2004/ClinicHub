namespace ClinicHub.Application.Features.Payment.DTOs;

public class WalletPaymentResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentKey { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}
