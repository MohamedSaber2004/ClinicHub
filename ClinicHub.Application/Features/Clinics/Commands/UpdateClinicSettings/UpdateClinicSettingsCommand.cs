using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.UpdateClinicSettings
{
    public record UpdateClinicSettingsCommand(
        string Name,
        string? ResponsibleDoctor,
        string? Description,
        string? Phone,
        string? ManagerName,
        string? Location,
        Guid SpecializationId,
        decimal ConsultationFee,
        string? Currency = null,
        int MaxAdvanceBookingDays = 30,
        int ReservationTtlMinutes = 10,
        int CancellationWindowMinutes = 120,
        double? Latitude = null,
        double? Longitude = null,
        bool IsActive = true) : IRequest<ClinicSettingsDto>;
}
