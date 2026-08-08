using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinic
{
    public record UpdateClinicCommand(
        Guid Id, 
        string? Name,
        string? NameAr,
        string? Description,
        string? ArDescription,
        string? Address,
        string? AddressAr,
        string? Phone,
        string Email,
        string? Website,
        string? Logo,
        string? WorkingHours,
        Guid? SpecializationId,
        TimeOnly? WorkingHoursStart = null,
        TimeOnly? WorkingHoursEnd = null,
        List<DayOfWeek>? WorkingDays = null,
        double? Latitude = null,
        double? Longitude = null) : IRequest<ClinicManagementDto>;
}
