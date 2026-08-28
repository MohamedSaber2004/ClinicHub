using ClinicHub.Application.Features.Ads.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Commands.UpdateClinicAdSettings;

public class UpdateClinicAdSettingsCommand : IRequest<ClinicAdSettingsDto>
{
    public Guid ClinicId { get; set; }
    public int MaxAds { get; set; }
    public int MaxImpressions { get; set; }
}
