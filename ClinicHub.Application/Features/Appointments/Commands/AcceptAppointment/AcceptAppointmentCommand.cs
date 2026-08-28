using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Commands.AcceptAppointment
{
    public class AcceptAppointmentCommand : IRequest<AppointmentAcceptanceResultDto>
    {
        public Guid AppointmentId { get; set; }
        /// <summary>Optional: "wallet" or "card"/"creditcard". Null = hosted checkout (both methods) for backward compat.</summary>
        public string? PaymentMethod { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
