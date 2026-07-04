using MediatR;

namespace ClinicHub.Application.Features.UserClinics.Commands.UnfollowClinic
{
    public class UnfollowClinicCommand : IRequest<bool>
    {
        public Guid ClinicId { get; set; }
    }
}
