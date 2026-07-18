namespace ClinicHub.Application.Features.Clinics.Commands.SetupClinic
{
    public record SetupClinicDto(
        string Name,
        string? Description,
        string? Address,
        string? Phone,
        string? Email,
        string? Website,
        string? Logo,
        string? WorkingHours,
        Guid SpecializationId,
        double Lat,
        double Lng,
        TimeOnly? WorkingHoursStart = null,
        TimeOnly? WorkingHoursEnd = null,
        List<DayOfWeek>? WorkingDays = null);
}
