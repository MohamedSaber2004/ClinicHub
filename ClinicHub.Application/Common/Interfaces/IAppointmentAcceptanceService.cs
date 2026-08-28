using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Common.Interfaces;

/// <summary>
/// Orchestrates the "accept appointment" lifecycle for every dashboard path
/// (clinic admin, staff, doctor). Sets status to Accepted, creates/refreshes the
/// appointment payment (type = Appointment), initiates the Paymob hosted checkout
/// and notifies the patient with the payment link.
/// </summary>
public interface IAppointmentAcceptanceService
{
    Task<AppointmentAcceptanceResultDto> AcceptAsync(Appointment appointment, CancellationToken cancellationToken, string? paymentMethod = null, string? returnUrl = null);
}
