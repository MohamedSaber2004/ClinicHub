using MediatR;

namespace ClinicHub.Application.Features.Doctors.Commands.DeleteDoctor
{
    public class DeleteDoctorCommand : IRequest<bool>
    {
        public Guid DoctorId { get; set; }
    }
}
