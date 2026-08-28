using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayments.Commands.CreateAdsOrder;

public class CreateAdsOrderCommand : IRequest<CreateAdsOrderResponseDto>
{
    public Guid ClinicId { get; set; }
    public Guid AdPackageId { get; set; }
    public int DurationDays { get; set; }
    public string? LogoImageUrl { get; set; }
    public string? ReturnUrl { get; set; }
    public string? PaymentMethod { get; set; }
}
