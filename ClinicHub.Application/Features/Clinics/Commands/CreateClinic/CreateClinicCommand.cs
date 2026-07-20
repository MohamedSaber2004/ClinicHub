using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.CreateClinic
{
    public record CreateClinicCommand(
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
        Guid SpecializationId,
        Guid DoctorSpecializationId,
        string OwnerName,
        string OwnerEmail,
        string? OwnerPhone,
        double? Lat = null,
        double? Lng = null,
        TimeOnly? WorkingHoursStart = null,
        TimeOnly? WorkingHoursEnd = null,
        List<DayOfWeek>? WorkingDays = null,
        string? Bio = null,
        int YearsOfExperience = 0) : IRequest<ClinicManagementDto>;
}
