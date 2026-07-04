using MediatR;

namespace ClinicHub.Application.Features.UserClinics.Commands.FollowClinic
{
    public class FollowClinicCommand : IRequest<bool>
    {
        public Guid ClinicId { get; set; }
    }
}
