using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Commands.CreateClinicAdOrder;

public class CreateClinicAdOrderCommand : IRequest<CreateAdsOrderResponseDto>
{
    public Guid ClinicId { get; set; }
    public Guid AdPackageId { get; set; }
    public int DurationDays { get; set; }
    public string? ReturnUrl { get; set; }
}
