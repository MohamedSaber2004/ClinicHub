using ClinicHub.Application.Features.UserClinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.UserClinics.Queries.GetFollowedClinics
{
    public class GetFollowedClinicsQuery : IRequest<List<FollowedClinicDto>>
    {
    }
}
