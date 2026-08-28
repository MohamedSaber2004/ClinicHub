using ClinicHub.Application.Features.AdminPayments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public class CreateAdPackageCommand : IRequest<AdPackageDto>
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public int MaxAds { get; set; }
    public int MaxImpressions { get; set; }
}
