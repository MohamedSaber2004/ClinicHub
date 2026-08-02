using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayments.DTOs;

public class CreateAdsOrderResponseDto
{
    public Guid PaymentId { get; set; }
    public string? RefNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentStatus Status { get; set; }
    public string? PaymobRedirectUrl { get; set; }
    public string? PaymobPaymentKey { get; set; }
}
