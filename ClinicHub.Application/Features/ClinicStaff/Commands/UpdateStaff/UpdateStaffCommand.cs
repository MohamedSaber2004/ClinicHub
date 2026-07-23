using MediatR;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.UpdateStaff
{
    public class UpdateStaffCommand : IRequest<bool>
    {
        public Guid StaffId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
