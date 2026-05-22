using MediatR;

namespace ClinicHub.Application.Features.Availability.Commands.DeleteAvailability
{
    public class DeleteAvailabilityCommand : IRequest<string>
    {
        public Guid Id { get; set; }
    }
}
