namespace ClinicHub.Application.Features.Ads.Commands.AdPackages;

public record AdPackagePayload(string Name, string? NameAr, string? Description, string? DescriptionAr, decimal Price, int DurationDays, bool IsActive);
