using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Commands.DeleteMyAvailability
{
    public class DeleteMyAvailabilityCommand : IRequest<string>
    {
        public Guid Id { get; set; }
    }
}
