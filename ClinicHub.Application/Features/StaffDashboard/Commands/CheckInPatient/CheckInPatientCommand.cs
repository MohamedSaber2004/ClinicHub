using MediatR;

namespace ClinicHub.Application.Features.StaffDashboard.Commands.CheckInPatient
{
    public class CheckInPatientCommand : IRequest<bool>
    {
        public Guid AppointmentId { get; set; }
    }
}
