using MediatR;

namespace ClinicHub.Application.Features.Users.Commands.AssignUserRole
{
    public class AssignUserRoleCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = null!;
    }
}
