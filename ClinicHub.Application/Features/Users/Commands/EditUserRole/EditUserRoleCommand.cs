using MediatR;

namespace ClinicHub.Application.Features.Users.Commands.EditUserRole
{
    public class EditUserRoleCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string NewRole { get; set; } = null!;
    }
}
