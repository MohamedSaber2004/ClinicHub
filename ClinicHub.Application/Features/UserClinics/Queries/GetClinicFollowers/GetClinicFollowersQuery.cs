using ClinicHub.Application.Features.UserClinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.UserClinics.Queries.GetClinicFollowers
{
    public class GetClinicFollowersQuery : IRequest<List<ClinicFollowerDto>>
    {
        public Guid ClinicId { get; set; }
    }
}
