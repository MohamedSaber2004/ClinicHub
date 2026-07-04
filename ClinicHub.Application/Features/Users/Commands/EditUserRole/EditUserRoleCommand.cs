using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Users.Commands.EditUserRole
{
    public class EditUserRoleCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public UserType NewRole { get; set; }
    }
}
