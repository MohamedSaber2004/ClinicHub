using ClinicHub.Application.Features.Availability.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.DoctorDashboard.Availability.Queries.GetMyAvailability
{
    public class GetMyAvailabilityQuery : IRequest<List<AvailabilityDto>>
    {
    }
}
