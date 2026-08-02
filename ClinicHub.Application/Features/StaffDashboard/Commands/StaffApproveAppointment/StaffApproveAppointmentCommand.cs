using ClinicHub.Application.Features.Appointments.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.StaffApproveAppointment
{
    public class StaffApproveAppointmentCommand : IRequest<AppointmentAcceptanceResultDto>
    {
        public Guid AppointmentId { get; set; }
    }
}
