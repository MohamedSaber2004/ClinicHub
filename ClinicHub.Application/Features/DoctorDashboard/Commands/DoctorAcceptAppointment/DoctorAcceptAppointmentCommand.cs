using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Commands.DoctorAcceptAppointment
{
    public class DoctorAcceptAppointmentCommand : IRequest<AppointmentAcceptanceResultDto>
    {
        public Guid AppointmentId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
