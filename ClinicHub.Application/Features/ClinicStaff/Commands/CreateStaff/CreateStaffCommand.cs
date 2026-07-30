using MediatR;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.CreateStaff
{
    public class CreateStaffCommand : IRequest<Guid>
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Image { get; set; }
    }
}
