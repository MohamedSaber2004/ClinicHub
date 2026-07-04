using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Users.Commands.AddUser
{
    public class AddUserCommand : IRequest<Guid>
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
        public UserType Role { get; set; }
    }
}
