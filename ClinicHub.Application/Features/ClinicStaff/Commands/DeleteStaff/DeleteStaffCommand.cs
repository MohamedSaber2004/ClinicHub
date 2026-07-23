using MediatR;

namespace ClinicHub.Application.Features.ClinicStaff.Commands.DeleteStaff
{
    public class DeleteStaffCommand : IRequest<bool>
    {
        public Guid StaffId { get; set; }
    }
}
