namespace ClinicHub.Application.Features.Ads.DTOs;

public class PublicAdDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicLogoUrl { get; set; }
    public Guid PackageId { get; set; }
    public string? PackageNameAr { get; set; }
    public string? Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
