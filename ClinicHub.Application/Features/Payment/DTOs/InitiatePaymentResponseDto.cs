namespace ClinicHub.Application.Features.Payment.DTOs;

public class InitiatePaymentResponseDto
{
    public string PaymentKey { get; set; } = string.Empty;
    public string IframeUrl { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
}