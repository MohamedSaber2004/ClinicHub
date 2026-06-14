using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Users.Commands.EditUser
{
    public class EditUserCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public bool? IsActive { get; set; }
    }
}
