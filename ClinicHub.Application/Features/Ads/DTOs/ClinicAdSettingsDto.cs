namespace ClinicHub.Application.Features.Ads.DTOs;

public class ClinicAdSettingsDto
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int MaxAds { get; set; }
    public int MaxImpressions { get; set; }
    public int ActiveAdsCount { get; set; }
}
